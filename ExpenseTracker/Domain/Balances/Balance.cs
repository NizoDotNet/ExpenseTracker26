using ExpenseTracker.Domain.Primitives;

namespace ExpenseTracker.Domain.Balances;

public class Balance : Entity<Guid>
{
    private Balance()
    {
        
    }
    private Balance(string name, decimal amount, Guid userId)
    {
        Name = name;
        Amount = amount;
        UserId = userId;
    }

    public string Name { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public Guid UserId { get; private set; }

    public void ApplyTransaction(decimal amount)
    {
        Amount += amount;
    }

    public static Balance Create(string name, decimal amount, Guid userId)
    {
        return new(name, amount, userId);
    }
}
