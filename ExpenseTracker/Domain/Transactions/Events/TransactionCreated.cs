using ExpenseTracker.Domain.Primitives;

namespace ExpenseTracker.Domain.Transactions.Events;

public class TransactionCreated : DomainEvent
{
    public TransactionCreated(Guid balanceId, decimal amount)
    {
        BalanceId = balanceId;
        Amount = amount;
    }

    public Guid BalanceId { get; }
    public decimal Amount { get; }

}
