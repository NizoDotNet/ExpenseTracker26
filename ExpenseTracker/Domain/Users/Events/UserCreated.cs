using ExpenseTracker.Domain.Primitives;

namespace ExpenseTracker.Domain.Users.Events;

public class UserCreated : DomainEvent
{
    public UserCreated(string email)
    {
        Email = email;
    }
    public string Email { get; }
}
