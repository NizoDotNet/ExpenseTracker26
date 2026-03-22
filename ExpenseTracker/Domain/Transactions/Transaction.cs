using ExpenseTracker.Domain.Primitives;

namespace ExpenseTracker.Domain.Transactions;

public class Transaction : Entity<Guid>
{
    private Transaction()
    {
        
    }

    private Transaction(string name, string? description, DateTimeOffset dateTime, decimal amount, int transactionTypeId)
    {
        Name = name;
        Description = description;
        DateTime = dateTime;
        Amount = amount;
        TransactionTypeId = transactionTypeId;
    }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTimeOffset DateTime { get; private set; }
    public decimal Amount { get; private set; }
    public int TransactionTypeId { get; private set; }
    public TransactionType TransactionType { get; init; }

    public static Transaction Create(string name, string? description, DateTimeOffset dateTime, decimal amount, int transactionTypeId)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        return new(name, description, dateTime, amount, transactionTypeId);
    }
}
