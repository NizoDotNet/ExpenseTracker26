using ExpenseTracker.Application.Transactions.Events;
using ExpenseTracker.Application.Transactions.Requests;
using ExpenseTracker.Domain.Transactions;
using ExpenseTracker.Domain.Transactions.Events;
using ExpenseTracker.Infrastracture;
using System.Runtime.InteropServices;

namespace ExpenseTracker.Services;

public class TransactionService 
{
    private readonly DatabaseContext _db;
    private readonly HttpContext _ctx;
    public TransactionService(DatabaseContext db, HttpContext ctx)
    {
        _db = db;
        _ctx = ctx;
    }

    public async Task<Transaction> InsertAsync(CreateTransationRequest createTransationRequest, CancellationToken cancellationToken = default)
    {
        // Validate Transaction

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
        return transaction;
    }
}
