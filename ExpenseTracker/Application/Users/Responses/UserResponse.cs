using ExpenseTracker.Domain.Users;

namespace ExpenseTracker.Application.Users.Responses;

public record UserResponse
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required BalanceResponse Balance { get; init; }
    public static UserResponse Map(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.UserName,
            Balance = BalanceResponse.Map(user.Balance)
        };
    }
}

public record BalanceResponse
{
    public required Guid Id { get; init; }
    public required decimal Amount { get; init; }
    public static BalanceResponse Map(Balance balance)
    {
        return new BalanceResponse
        {
            Id = balance.Id,
            Amount = balance.Amount,
        };
    }
}