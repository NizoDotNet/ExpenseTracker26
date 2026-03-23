namespace ExpenseTracker.Application.Transactions.Responses;

public record TransactionResponse(Guid Id, string Name, string Description, DateTimeOffset dateTime, decimal Amount, TransactionCategory TransactionCategory);

public record TransactionCategory(Guid Id, string CategoryName);
