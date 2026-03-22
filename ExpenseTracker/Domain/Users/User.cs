using ExpenseTracker.Domain.Primitives;

namespace ExpenseTracker.Domain.Users;

public class User : Entity<Guid>
{
    private User()
    {
    }
    internal User(string email, string userName, string password)
    {
        Email = email;
        UserName = userName;
        Password = password;
    }
    public string Email { get; private set; } = null!;
    public string UserName { get; private set; } = null!;
    public string Password { get; private set; } = null!;

    public static User Create(string email, string? username, string password)
    {
        if(string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException("email");
        if(string.IsNullOrWhiteSpace(username)) username = email;
        if (string.IsNullOrWhiteSpace(password) && password.Length < 5)
            throw new Exception("Password min length is 5");

        return new(email, username, password);
    }

}
