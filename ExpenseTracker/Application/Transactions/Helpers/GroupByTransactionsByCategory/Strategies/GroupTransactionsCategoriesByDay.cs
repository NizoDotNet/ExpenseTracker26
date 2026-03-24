using ExpenseTracker.Application.Transactions.Responses;
using ExpenseTracker.Infrastracture;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByCategory.Strategies;

public class GroupTransactionsCategoriesByDay : IGroupByTransactionsByCategory
{
    public async Task<List<TransactionExpenseByCategoryResponse>> Handle(DatabaseContext dbContext, Guid balanceId, DateTimeOffset dateTime, CancellationToken cancellationToken = default)
    {
        int diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;

        DateTimeOffset startOfWeek = dateTime.UtcDateTime.AddDays(-diff);
        DateTimeOffset endOfWeek = startOfWeek.AddDays(7);

        var query = dbContext.Transactions
            .AsNoTracking()
            .Where(c => c.BalanceId == balanceId)
            .Where(t => t.DateTime >= startOfWeek && t.DateTime < endOfWeek);

        var transactions = await query
            .Where(c => c.Amount < 0)
            .Include(c => c.TransactionCategory)
            .GroupBy(c => c.TransactionCategory.Name)
            .Select(c => new TransactionExpenseByCategoryResponse(
                c.Key,
                c.Sum(c => c.Amount)))
            .ToListAsync(cancellationToken);

        return transactions;
    }
}
