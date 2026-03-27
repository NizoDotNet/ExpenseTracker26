using ExpenseTracker.Application.Shared;
using ExpenseTracker.Application.Shared.Enums;
using ExpenseTracker.Application.Transactions.Events;
using ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByCategory;
using ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByCategory.Strategies;
using ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByTimePeriod;
using ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByTimePeriod.Strategies;
using ExpenseTracker.Application.Transactions.Requests;
using ExpenseTracker.Application.Transactions.Responses;
using ExpenseTracker.Application.Users.Responses;
using ExpenseTracker.Domain.Transactions;
using ExpenseTracker.Domain.Transactions.Events;
using ExpenseTracker.Domain.Users;
using ExpenseTracker.Infrastracture;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services;

public class TransactionService(
    DatabaseContext db,
    IHttpContextAccessor ctxAccessor,
    IValidator<CreateTransationRequest> createTransactionValidator,
    IValidator<UpdateTransactionRequest> updateTransactionValidator)
{
    private readonly HttpContext _ctx = ctxAccessor.HttpContext!;
    
    public Task<List<TransactionCategory>> GetTransactionCategoriesAsync()
    {
        return db.TransactionTypes
            .ToListAsync();
    }
    public async Task<Result<Transaction?>> InsertAsync(Guid balanceId, CreateTransationRequest createTransationRequest, CancellationToken cancellationToken = default)
    {
        // Validate Transaction
        var validation = createTransactionValidator.Validate(createTransationRequest);

        if (!validation.IsValid)
        {
            return Result<Transaction?>.Failed(null, validation.ToDictionary());
        }
        Transaction transaction = Transaction.Create(
            balanceId,
            createTransationRequest.Name,
            createTransationRequest.Description,
            createTransationRequest.DateTime,
            createTransationRequest.Amount,
            createTransationRequest.TransactionCategoryId);

        await db.Transactions.AddAsync(transaction);

        // Need add domain event dispatcher later
        TransactionCreatedHandler transactionCreatedHandler = new(db, _ctx);
        await transactionCreatedHandler.Handle((TransactionCreated)transaction.Events[0], cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
        return Result<Transaction?>.Succeed(transaction);
    }

    public async Task<PagedResult<TransactionResponse>> GetAllAsyncWithPagination(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var balanceId = await db.Balances
            .Where(c => c.UserId == userId)
            .AsNoTracking()
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        var transactionsQuery = db.Transactions
            .AsNoTracking()
            .Where(c => c.BalanceId == balanceId)
            .Include(c => c.TransactionCategory)
            .OrderByDescending(c => c.DateTime)
            .Select(c => new TransactionResponse(
                c.Id,
                c.Name,
                c.Description ?? string.Empty,
                c.DateTime,
                c.Amount,
                new TransactionCategoryResponse(c.TransactionCategory.Id, c.TransactionCategory.Name)))
            .AsQueryable();

        return await PagedResult<TransactionResponse>.Create(transactionsQuery, page, pageSize);
    }

    public async Task<List<TransactionTimePeriodResponse>> GetTransactionTimePeriodResponsesAsync(Guid balanceId, TimePeriod timePeriod, bool? isIncome = null, DateTimeOffset? dateTime = null, CancellationToken cancellationToken = default)
    {
        dateTime ??= DateTimeOffset.UtcNow;

        IGroupByTransactionByTimePeriod groupByTransactionByTimePeriod = GetStrategyForGroupingByTimePeriod(timePeriod);

        return await groupByTransactionByTimePeriod.Handle(db, balanceId, ((DateTimeOffset)dateTime).ToUniversalTime(), isIncome, cancellationToken);
    }

    public async Task<Result<Transaction?>> DeleteAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken)
    {
        Transaction? transaction;
        (bool flowControl, Result<Transaction?> result) = await IsTransactionBelongsToUser(transactionId, userId);
        if (!flowControl || result.Value is null)
        {
            return result;
        }

        db.Transactions.Remove(result.Value);
        int res = await db.SaveChangesAsync(cancellationToken);
        return Result<Transaction?>.Succeed(result.Value);
    }
    public async Task<List<TransactionExpenseByCategoryResponse>> GetExpensesByCategory(Guid userId, TimePeriod timePeriod, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        var balanceId = await db.Balances
            .Where(c => c.UserId == userId)
            .AsNoTracking()
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        if(balanceId == default)
        {
            return [];
        }

        var transactions = await (GetStrategyForGroupingByCategory(timePeriod)).Handle(db, balanceId, date ?? DateTimeOffset.UtcNow, cancellationToken);

        return transactions;
    }
    public async Task<Result<Transaction?>> UpdateAsync(Guid transactionId, Guid userId, UpdateTransactionRequest updateTransactionRequest, CancellationToken cancellationToken)
    {
        var validation = updateTransactionValidator.Validate(updateTransactionRequest);

        if (!validation.IsValid)
        {
            return Result<Transaction?>.Failed(null, validation.ToDictionary());
        }
        (bool flowControl, Result<Transaction?> result) = await IsTransactionBelongsToUser(transactionId, userId);
        if (!flowControl || result.Value is null)
        {
            return result;
        }

        var transaction = result.Value;
        transaction.Update(updateTransactionRequest.Name,
            updateTransactionRequest.Description,
            updateTransactionRequest.DateTime,
            updateTransactionRequest.Amount,
            updateTransactionRequest.TransactionCategoryId);

        await db.SaveChangesAsync();
        return Result<Transaction?>.Succeed(transaction);

    }
    public async Task<UserBalanceCalculatedStats> GetUsersCalculatedBalanceStats(Guid userId)
    {
        // Get balance
        Balance? currentBalance = await db.Balances
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .FirstOrDefaultAsync();
        if (currentBalance == null)
        {
            return new(0, 0, 0, 0, 0);
        }

        // Calculate previouse month balance
        var now = DateTimeOffset.UtcNow;
        var previouseMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        decimal previousMonthTransactionsSum = await _db.Transactions
            .AsNoTracking()
            .Where(c => c.BalanceId == currentBalance.Id &&
                    c.DateTime <= now &&
                    c.DateTime >= previouseMonth)
            .SumAsync(c => c.Amount);

        decimal previouseMonthBalance = currentBalance.Amount - previousMonthTransactionsSum;

        // Income and Expense
        var result = await db.Transactions
            .AsNoTracking()
            .Where(c => c.BalanceId == currentBalance.Id &&
                        c.DateTime >= previouseMonth &&
                        c.DateTime <= now)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Income = g.Where(x => x.Amount > 0).Sum(x => x.Amount),
                Expense = g.Where(x => x.Amount < 0).Sum(x => x.Amount)
            })
            .FirstOrDefaultAsync();

        decimal income = result?.Income ?? 0;
        decimal expense = result?.Expense ?? 0;

        // Saving rate

        decimal savingRate = ((income - expense) / income) * 100;

        return new(currentBalance.Amount, currentBalance.Amount - previouseMonthBalance, income, expense, savingRate);
    }
    private async Task<(bool flowControl, Result<Transaction?> result)> IsTransactionBelongsToUser(Guid transactionId, Guid userId)
    {
        Guid balanceId = await db.Balances
                    .Where(c => c.UserId == userId)
                    .AsNoTracking()
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync();

        if (balanceId == default)
        {
            return (flowControl: false, result: Result<Transaction?>.Failed(null, new Dictionary<string, string[]>()));
        }

        var transaction = await db.Transactions
            .Where(c => c.Id == transactionId)
            .FirstOrDefaultAsync();
        if (transaction == null)
        {
            return (flowControl: false, result: Result<Transaction?>.Failed(null, new Dictionary<string, string[]>()));
        }

        if (transaction.BalanceId != balanceId)
        {
            return (flowControl: false, result: Result<Transaction?>.Failed(null, new Dictionary<string, string[]>()));
        }

        return (flowControl: true, result: Result<Transaction?>.Succeed(transaction));
    }
    private IGroupByTransactionByTimePeriod GetStrategyForGroupingByTimePeriod(TimePeriod timePeriod)
        => timePeriod switch
        {
            TimePeriod.Day => new GroupTransactionsByDay(),
            TimePeriod.Month => new GroupByTransactionsByMonth()
        };
    private IGroupByTransactionsByCategory GetStrategyForGroupingByCategory(TimePeriod timePeriod) 
        => timePeriod switch
    {
        TimePeriod.Day => new GroupTransactionsCategoriesByDay(),
        TimePeriod.Month => new GroupTransactionsCategoriesByMonth()
    };

}
