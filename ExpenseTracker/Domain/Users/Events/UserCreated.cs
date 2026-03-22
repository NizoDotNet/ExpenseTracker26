using ExpenseTracker.Domain.Primitives;

namespace ExpenseTracker.Domain.Users.Events;

public class UserCreated : DomainEvent
{
    public UserCreated(Guid id)
    {
        UserId = id;
    }
    public Guid UserId { get; }
}
