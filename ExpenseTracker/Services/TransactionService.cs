using ExpenseTracker.Application.Shared;
using ExpenseTracker.Application.Transactions.Events;
using ExpenseTracker.Application.Transactions.Requests;
using ExpenseTracker.Domain.Transactions;
using ExpenseTracker.Domain.Transactions.Events;
using ExpenseTracker.Infrastracture;
using FluentValidation;

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

        await _db.SaveChangesAsync();
        return Result<Transaction?>.Succeed(transaction);
    }
}
