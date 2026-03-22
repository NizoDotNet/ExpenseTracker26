using ExpenseTracker.Domain.Primitives;

namespace ExpenseTracker.Domain.Transactions;

public class TransactionType : Entity<int>
{
    private TransactionType()
    {
        
    }
    private TransactionType(string name)
    {
        Name = name;
    }

    public string Name { get; private set; } = null!;

    

    public static TransactionType Create(string name)
    {
        if(string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        return new(name);
    }
}
