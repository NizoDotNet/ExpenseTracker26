using ExpenseTracker.Application.Users.Requests;
using ExpenseTracker.Application.Users.Responses;
using ExpenseTracker.Domain.Users;
using ExpenseTracker.Infrastracture;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services;

public class UserService 
{
    private readonly DatabaseContext _db;

    public UserService(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<User> RegisterAsync(CreateUserRequest createUser)
    {
        // TODO: Write validation

        User user = User.Create(createUser.Email, createUser.Username);
        user.SetPassword(createUser.Password);

        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<UserResponse?> GetAsync(Guid userId, bool tracking)
    {
        IQueryable<User> userQuery = _db.Users;

        if(!tracking)
        {
            userQuery = userQuery
                .AsNoTracking();
        }

        return await userQuery
            .Select(c => new UserResponse(
                c.Id,
                c.Email,
                c.UserName,
                new BalanceResponse(c.Balance.Amount)))
            .FirstOrDefaultAsync(c => c.Id == userId);
    }
}

