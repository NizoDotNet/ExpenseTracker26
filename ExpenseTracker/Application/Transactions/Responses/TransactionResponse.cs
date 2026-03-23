namespace ExpenseTracker.Application.Transactions.Responses;

public record TransactionResponse(Guid Id, string Name, string Description, DateTimeOffset dateTime, decimal Amount, TransactionCategoryResponse Category);

public record TransactionCategoryResponse(int Id, string CategoryName);
