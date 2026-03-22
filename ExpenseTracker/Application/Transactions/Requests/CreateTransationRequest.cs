using Microsoft.OpenApi;

namespace ExpenseTracker.Application.Transactions.Requests;

public record CreateTransationRequest(string Name, string? Description, DateTimeOffset DateTime, decimal Amount, Guid BalanceId, int TransactionTypeId);
