using ExpenseTracker.Domain.Primitives;
using System.Runtime.CompilerServices;

namespace ExpenseTracker.Domain.Users;

public class Balance : Entity<Guid>
{
    private Balance()
    {
        Id = Guid.NewGuid();
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
