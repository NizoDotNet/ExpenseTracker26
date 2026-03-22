using ExpenseTracker.Application.Abstractions;
using ExpenseTracker.Domain.Transactions.Events;
using ExpenseTracker.Infrastracture;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseTracker.Application.Transactions.Events;

public class TransactionCreatedHandler : IDomainEventHandler<TransactionCreated>
{
    private readonly DatabaseContext _db;
    private readonly HttpContext _ctx;

    public TransactionCreatedHandler(DatabaseContext db, HttpContext ctx)
    {
        _db = db;
        _ctx = ctx;
    }

    public async Task Handle(TransactionCreated domainEvent, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .Include(c => c.Balance)
            .FirstOrDefaultAsync(c => c.Balance.Id == domainEvent.BalanceId);

        var userId = _ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null || user == null || Guid.Parse(userId) != user.Id)
        {
            throw new Exception("Problem with transaction");
        }

        user.Balance.ApplyTransaction(domainEvent.Amount);
    }
}
