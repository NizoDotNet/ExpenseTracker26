using ExpenseTracker.Domain.Primitives;

namespace ExpenseTracker.Domain.Balances;

public class Balance : Entity<Guid>
{
    public decimal Amount { get; private set; }
    public Guid UserId { get; private set; }

    public void ApplyTransaction(decimal amount)
    {
        Amount += amount;
    }
}
