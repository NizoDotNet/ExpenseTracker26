namespace ExpenseTracker.Application.Users.Requests;

public record CreateUserRequest(string Email, string? Username, string Password);
