using ExpenseTracker.Application.Shared;
using ExpenseTracker.Application.Users.Requests;
using ExpenseTracker.Application.Users.Responses;
using ExpenseTracker.Domain.Helpers;
using ExpenseTracker.Domain.Transactions;
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

    public async Task<Result<User?>> RegisterAsync(CreateUserRequest createUser, CancellationToken cancellationToken = default)
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
        await _db.SaveChangesAsync(cancellationToken);

        return Result<User?>.Succeed(user);
    }
    public async Task<UserResponse?> LoginUser(string email, string password, CancellationToken cancellationToken = default)
    {
        password = PasswordHashingHelper.HashPassword(password);

        var user = await _db.Users
            .AsNoTracking()
            .Include(c => c.Balance)
            .FirstOrDefaultAsync(c => c.Email == email && c.Password == password, cancellationToken);

        if (user == null)
            return null;


        return UserResponse.Map(user);
    }
    public async Task<UserResponse?> GetAsync(Guid userId, bool tracking)
    {
        IQueryable<User> userQuery = _db.Users
            .Where(c => c.Id == userId);

        if (!tracking)
        {
            userQuery = userQuery
                .AsNoTracking();
        }

        return await userQuery
            .Include(c => c.Balance)
            .Select(c => UserResponse.Map(c))
            .FirstOrDefaultAsync();
    }
    public async Task<Guid?> GetUserBalanceId(Guid userId)
    {
        return await _db.Balances
            .Where(c => c.UserId == userId)
            .Select(c => c.Id)
            .FirstOrDefaultAsync();
    }
    public async Task<UserBalanceCalculatedStats> GetUsersCalculatedBalanceStats(Guid userId)
    {
        // Get balance
        Balance? currentBalance = await _db.Balances
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .FirstOrDefaultAsync();
        if(currentBalance == null)
        {
            return new(0, 0, 0, 0, 0);
        }

        // Calculate previouse month balance
        var now = DateTimeOffset.UtcNow;
        var previouseMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        decimal previousMonthTransactionsSum = await _db.Transactions
            .AsNoTracking()
            .Where(c => c.BalanceId == currentBalance.Id &&
                    c.DateTime <=  now && 
                    c.DateTime >= previouseMonth)
            .SumAsync(c => c.Amount);

        decimal previouseMonthBalance = currentBalance.Amount - previousMonthTransactionsSum;

        // Income and Expense
        var result = await _db.Transactions
            .AsNoTracking()
            .Where(c => c.BalanceId == currentBalance.Id &&
                        c.DateTime >= previouseMonth &&
                        c.DateTime <= now)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Income = g.Where(x => x.Amount > 0).Sum(x => x.Amount),
                Expense = g.Where(x => x.Amount < 0).Sum(x => x.Amount)
            })
            .FirstOrDefaultAsync();

        decimal income = result?.Income ?? 0;
        decimal expense = result?.Expense ?? 0;

        // Saving rate

        decimal savingRate = ((income - expense) / income) * 100;

        return new(currentBalance.Amount, currentBalance.Amount - previouseMonthBalance, income, expense, savingRate);
    }
}

