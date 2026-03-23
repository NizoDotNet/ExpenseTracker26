using ExpenseTracker.Domain.Helpers;
using ExpenseTracker.Domain.Primitives;
using ExpenseTracker.Domain.Users.Events;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace ExpenseTracker.Domain.Users;

public class User : Entity<Guid>
{
    private readonly List<DomainEvent> _events = new();
    private User()
    {
    }
    internal User(string email, string userName)
    {
        Id = Guid.NewGuid();
        Email = email;
        UserName = userName;
        Balance = Balance.Create();
        
    }
    public string Email { get; private set; } = null!;
    public string UserName { get; private set; } = null!;
    public string Password { get; private set; } = null!;
    public Guid BalanceId { get; init; }
    public Balance Balance { get; private set; }
    public IReadOnlyList<DomainEvent> DomainEvents => _events.AsReadOnly();

    public void Raise(DomainEvent domainEvent)
    {
        _events.Add(domainEvent);
    }

    public void SetPassword(string password)
    {
        password = PasswordHashingHelper.HashPassword(password);

        Password = password;
    }
    public static User Create(string email, string? username)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException("email");
        if (string.IsNullOrWhiteSpace(username)) username = email;

        User user = new(email, username);
        user.Raise(new UserCreated(user.Id));
        return user;
    }

}
