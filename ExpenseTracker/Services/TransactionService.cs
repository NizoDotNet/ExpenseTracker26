using ExpenseTracker.Application.Shared;
using ExpenseTracker.Application.Transactions.Events;
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
    public TransactionService(DatabaseContext db, HttpContext ctx, IValidator<CreateTransationRequest> validator)
    {
        _db = db;
        _ctx = ctx;
        _validator = validator;
    }

    public async Task<Result<Transaction?>> InsertAsync(CreateTransationRequest createTransationRequest, CancellationToken cancellationToken = default)
    {
        // Validate Transaction
        var validation = _validator.Validate(createTransationRequest);

        if(!validation.IsValid)
        {
            return Result<Transaction?>.Failed(null, validation.ToDictionary());
        }
        Transaction transaction = Transaction.Create(
            createTransationRequest.BalanceId,
            createTransationRequest.Name,
            createTransationRequest.Description,
            createTransationRequest.DateTime,
            createTransationRequest.Amount,
            createTransationRequest.TransactionTypeId);

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
}
