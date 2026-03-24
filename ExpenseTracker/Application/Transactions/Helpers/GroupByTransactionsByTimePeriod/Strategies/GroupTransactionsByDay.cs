using ExpenseTracker.Application.Transactions.Responses;
using ExpenseTracker.Infrastracture;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByTimePeriod.Strategies;

public class GroupTransactionsByDay : IGroupByTransactionByTimePeriod
{
    public async Task<List<TransactionTimePeriodResponse>> Handle(DatabaseContext dbContext, Guid balanceId, DateTimeOffset dateTime, bool? isIncome = null, CancellationToken cancellationToken = default)
    {
        int diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;

        var startOfWeek = dateTime.Date.AddDays(-diff);
        var endOfWeek = startOfWeek.AddDays(7);

        var query = dbContext.Transactions
            .AsNoTracking()
            .Where(c => c.BalanceId == balanceId)
            .Where(t => t.DateTime >= startOfWeek && t.DateTime < endOfWeek);

        if (isIncome != null)
        {
            query = query.Where(c => (bool)isIncome ? c.Amount > 0 : c.Amount < 0);
        }
        var grouped = await query
            .Where(t => t.DateTime >= startOfWeek && t.DateTime < endOfWeek)
            .GroupBy(t => t.DateTime.Date)
            .Select(g => new
            {
                Date = g.Key,
                Total = Math.Abs(g.Sum(x => x.Amount))
            })
            .ToListAsync();

        return Enumerable.Range(0, 7)
            .Select(i => startOfWeek.AddDays(i))
            .GroupJoin(grouped,
                d => d,
                g => g.Date,
                (date, g) => new
                {
                    Date = date,
                    Total = g.FirstOrDefault()?.Total ?? 0
                })
            .OrderBy(x => x.Date)
            .Select(c => new TransactionTimePeriodResponse(c.Total, c.Date.DayOfWeek.ToString()))
            .ToList();
    }
}
