using ExpenseTracker.Application.Shared;
using ExpenseTracker.Application.Shared.Enums;
using ExpenseTracker.Application.Transactions.Events;
using ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByTimePeriod;
using ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByTimePeriod.Strategies;
using ExpenseTracker.Application.Transactions.Requests;
using ExpenseTracker.Application.Transactions.Responses;
using ExpenseTracker.Domain.Transactions;
using ExpenseTracker.Domain.Transactions.Events;
using ExpenseTracker.Infrastracture;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services;

public class TransactionService
{
    private readonly DatabaseContext _db;
    private readonly HttpContext _ctx;
    private readonly IValidator<CreateTransationRequest> _validator;
    public TransactionService(DatabaseContext db, IHttpContextAccessor ctxAccessor, IValidator<CreateTransationRequest> validator)
    {
        _db = db;
        _ctx = ctxAccessor.HttpContext!;
        _validator = validator;
    }

    public Task<List<TransactionCategory>> GetTransactionCategoriesAsync()
    {
        return _db.TransactionTypes
            .ToListAsync();
    }
    public async Task<Result<Transaction?>> InsertAsync(Guid balanceId, CreateTransationRequest createTransationRequest, CancellationToken cancellationToken = default)
    {
        // Validate Transaction
        var validation = _validator.Validate(createTransationRequest);

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

        await _db.Transactions.AddAsync(transaction);

        // Need add domain event dispatcher later
        TransactionCreatedHandler transactionCreatedHandler = new(_db, _ctx);
        await transactionCreatedHandler.Handle((TransactionCreated)transaction.Events[0], cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return Result<Transaction?>.Succeed(transaction);
    }

    public async Task<PagedResult<TransactionResponse>> GetAllAsyncWithPagination(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var balanceId = await _db.Balances
            .Where(c => c.UserId == userId)
            .AsNoTracking()
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        var transactionsQuery = _db.Transactions
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

        IGroupByTransactionByTimePeriod groupByTransactionByTimePeriod = GetStrategy(timePeriod);

        return await groupByTransactionByTimePeriod.Handle(_db, balanceId, ((DateTimeOffset)dateTime).ToUniversalTime(), isIncome, cancellationToken);
    }

    public async Task<Result<int>> DeleteAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken)
    {
        Guid balanceId = await _db.Balances
            .Where(c => c.UserId == userId)
            .AsNoTracking()
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        if(balanceId == default)
        {
            return Result<int>.Failed(-1, new Dictionary<string, string[]>());
        }

        var transaction = await _db.Transactions
            .Where(c => c.Id == transactionId)
            .FirstOrDefaultAsync();

        if(transaction == null)
        {
            return Result<int>.Failed(-1, new Dictionary<string, string[]>());
        }

        if(transaction.BalanceId != balanceId)
        {
            return Result<int>.Failed(-1, new Dictionary<string, string[]>());
        }

        _db.Transactions.Remove(transaction);
        int res = await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Succeed(res);
    }
    private IGroupByTransactionByTimePeriod GetStrategy(TimePeriod timePeriod)
        => timePeriod switch
        {
            TimePeriod.Day => new GroupTransactionsByDay(),
            TimePeriod.Month => new GroupByTransactionsByMonth()
        };
}
