using ExpenseTracker.Domain.Primitives;

namespace ExpenseTracker.Domain.Users;

public class Balance : Entity<Guid>
{
    private Balance()
    {
        Amount = 0.00m;
    }

    public decimal Amount { get; private set; }
    public User User { get; init; }
    public Guid UserId { get; private set; }

    public void ApplyTransaction(decimal amount)
    {
        Amount += amount;
    }

    public static Balance Create()
    {
        return new();
    }
}
