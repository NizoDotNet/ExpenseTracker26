using ExpenseTracker.Application.Transactions.Responses;
using ExpenseTracker.Infrastracture;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByTimePeriod.Strategies;

public class GroupTransactionsByWeek : IGroupByTransactionByTimePeriod
{
    public async Task<List<TransactionTimePeriodResponse>> Handle(DatabaseContext dbContext, DateTimeOffset dateTime, CancellationToken cancellationToken = default)
    {
        int diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;

        var startOfWeek = dateTime.Date.AddDays(-diff);
        var endOfWeek = startOfWeek.AddDays(7);

        var grouped = await dbContext.Transactions
            .Where(t => t.DateTime >= startOfWeek && t.DateTime < endOfWeek)
            .GroupBy(t => t.DateTime.Date)
            .Select(g => new
            {
                Date = g.Key,
                Total = g.Sum(x => x.Amount)
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
