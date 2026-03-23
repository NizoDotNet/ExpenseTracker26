using ExpenseTracker.Domain.Primitives;
using ExpenseTracker.Domain.Transactions.Events;

namespace ExpenseTracker.Domain.Transactions;

public class Transaction : Entity<Guid>
{
    private readonly List<DomainEvent> _events = new();
    private Transaction()
    {

    }

    private Transaction(Guid balanceId, string name, string? description, DateTimeOffset dateTime, decimal amount, int transactionCategoryId)
    {
        Name = name;
        Description = description;
        DateTime = dateTime;
        Amount = amount;
        BalanceId = balanceId;
        TransactionCategoryId = transactionCategoryId;
    }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTimeOffset DateTime { get; private set; }
    public decimal Amount { get; private set; }
    public Guid BalanceId { get; set; }
    public int TransactionCategoryId { get; private set; }
    public TransactionCategory TransactionCategory { get; init; }
    public IReadOnlyList<DomainEvent> Events => _events.AsReadOnly();
    public void Raise(DomainEvent domainEvent)
    {
        _events.Add(domainEvent);
    }

    public static Transaction Create(Guid balanceId, string name, string? description, DateTimeOffset dateTime, decimal amount, int transactionCategoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        Transaction transaction = new(balanceId, name, description, dateTime, amount, transactionCategoryId);
        transaction.Raise(new TransactionCreated(balanceId, amount));
        return transaction;
    }
}
