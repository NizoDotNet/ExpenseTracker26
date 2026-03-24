namespace ExpenseTracker.Application.Transactions.Responses;

public record TransactionTimePeriodResponse
{
    private decimal _amount;
    public TransactionTimePeriodResponse(decimal amount, string label)
    {
        Amount = amount;
        Label = label;
    }

    public string Label { get; }
    public decimal Amount { get => _amount; private set => _amount = value; }

}

public record TransactionIncomeExpenseResponse
{
    public List<string> Labels { get; set; } = [];
    public List<TransactionExpenseAmount> Expense { get; set; } = [];
    public List<TransactionExpenseAmount> Income { get; set; } = [];
}

public record TransactionExpenseAmount(decimal Amount);