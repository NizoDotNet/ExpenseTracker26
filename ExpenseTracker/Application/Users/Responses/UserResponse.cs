namespace ExpenseTracker.Application.Users.Responses;

public record UserResponse(Guid Id, string Email, string Username, BalanceResponse Balance);

public record BalanceResponse(decimal Amount);