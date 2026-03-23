using ExpenseTracker.Application.Shared;
using ExpenseTracker.Application.Users.Requests;
using ExpenseTracker.Application.Users.Responses;
using ExpenseTracker.Domain.Helpers;
using ExpenseTracker.Domain.Users;
using ExpenseTracker.Infrastracture;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services;

public class UserService
{
    private readonly DatabaseContext _db;
    private readonly IValidator<CreateUserRequest> _createUserRequestValidator;

    public UserService(DatabaseContext db, IValidator<CreateUserRequest> createUserRequestValidator)
    {
        _db = db;
        _createUserRequestValidator = createUserRequestValidator;
    }

    public async Task<Result<User?>> RegisterAsync(CreateUserRequest createUser)
    {
        // TODO: Write validation
        var validation = _createUserRequestValidator.Validate(createUser);
        if (!validation.IsValid)
        {
            return Result<User?>.Failed(null, validation.ToDictionary());
        }
        User user = User.Create(createUser.Email, createUser.Username);
        user.SetPassword(createUser.Password);

        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();

        return Result<User?>.Succeed(user);
    }

    public async Task<UserResponse?> LoginUser(string email, string password)
    {
        password = PasswordHashingHelper.HashPassword(password);

        var user = await _db.Users
            .AsNoTracking()
            .Include(c => c.Balance)
            .FirstOrDefaultAsync(c => c.Email == email && c.Password == password);

        if (user == null) 
            return null;


        return new UserResponse(user.Id, user.Email, user.UserName, new(user.Balance.Amount));
    }

    public async Task<UserResponse?> GetAsync(Guid userId, bool tracking)
    {
        IQueryable<User> userQuery = _db.Users;

        if (!tracking)
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

