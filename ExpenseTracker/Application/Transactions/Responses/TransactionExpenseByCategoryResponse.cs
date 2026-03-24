namespace ExpenseTracker.Application.Transactions.Responses;

public record TransactionExpenseByCategoryResponse(string Category, decimal Amount);