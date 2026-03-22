using ExpenseTracker.Domain.Primitives;

namespace ExpenseTracker.Domain.Transactions;

public class TransactionCategory : Entity<int>
{
    private TransactionCategory()
    {
        
    }
    private TransactionCategory(string name)
    {
        Name = name;
    }

    public string Name { get; private set; } = null!;

    

    public static TransactionCategory Create(string name)
    {
        if(string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        return new(name);
    }
}
