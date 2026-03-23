using ExpenseTracker.Application.Shared.Enums;

namespace ExpenseTracker.Application.Transactions.Responses;

public record TransactionTimePeriodResponse
{
    private decimal _amount;
    public TransactionTimePeriodResponse(Guid id, decimal amount, string time, TransactionCategoryResponse category)
    {
        Id = id;
        Amount = amount;
        Time = time;
        Category = category;
    }
    public Guid Id { get; private set; }
    public decimal Amount { get => _amount; private set => _amount = value; }
    public TransactionType Type => _amount > 0 ? TransactionType.Income : TransactionType.Expense;
    public string Time { get; set; }
    public TransactionCategoryResponse Category { get; set; }

}
