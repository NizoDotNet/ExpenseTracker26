using ExpenseTracker.Domain.Primitives;
using ExpenseTracker.Domain.Users.Events;

namespace ExpenseTracker.Domain.Users;

public class User : Entity<Guid>
{
    private readonly List<DomainEvent> _events = new();
    private User()
    {
        Balance = Users.Balance.Create();
    }
    internal User(string email, string userName, string password)
    {
        Id = Guid.NewGuid();
        Email = email;
        UserName = userName;
        Password = password;
    }
    public string Email { get; private set; } = null!;
    public string UserName { get; private set; } = null!;
    public string Password { get; private set; } = null!;
    public Guid BalanceId { get; init; }
    public Balance Balance { get; init; }
    public IReadOnlyList<DomainEvent> DomainEvents => _events.AsReadOnly();

    public void Raise(DomainEvent domainEvent)
    {
        _events.Add(domainEvent);
    }

    public static User Create(string email, string? username, string password)
    {
        if(string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException("email");
        if(string.IsNullOrWhiteSpace(username)) username = email;
        if (string.IsNullOrWhiteSpace(password) && password.Length < 5)
            throw new Exception("Password min length is 5");

        User user = new(email, username, password);
        user.Raise(new UserCreated(user.Id));
        return user;
    }

}
