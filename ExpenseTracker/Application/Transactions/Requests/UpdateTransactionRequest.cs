namespace ExpenseTracker.Application.Transactions.Requests;

public record UpdateTransactionRequest(
    string Name,
    string? Description,
    decimal Amount,
    int TransactionCategoryId)
{
    public DateTimeOffset DateTime { get => field; set => field = value.ToUniversalTime(); }
}
