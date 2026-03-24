using ExpenseTracker.Application.Transactions.Responses;
using ExpenseTracker.Infrastracture;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByTimePeriod.Strategies;

public class GroupByTransactionsByMonth : IGroupByTransactionByTimePeriod
{
    public async Task<List<TransactionTimePeriodResponse>> Handle(DatabaseContext dbContext, Guid balanceId, DateTimeOffset dateTime, bool? isIncome = null, CancellationToken cancellationToken = default)
    {

        var startOfYear = new DateTime(dateTime.Year, 1, 1).ToUniversalTime();
        var endOfYear = startOfYear.AddYears(1);

        var query = dbContext.Transactions
            .AsNoTracking()
            .Where(c => c.BalanceId == balanceId)
            .Where(t => t.DateTime >= startOfYear && t.DateTime < endOfYear);

        if (isIncome != null)
        {
            query = query.Where(c => (bool)isIncome ? c.Amount > 0 : c.Amount < 0);
        }
        var grouped = await query
            .GroupBy(t => t.DateTime.Month)
            .Select(g => new
            {
                Month = g.Key,
                Total = Math.Abs(g.Sum(x => x.Amount))
            })
            .ToListAsync(cancellationToken);

        return Enumerable.Range(1, 12)
            .GroupJoin(grouped,
                month => month,
                g => g.Month,
                (month, g) =>
                {
                    var item = g.FirstOrDefault();
                    var date = new DateTime(dateTime.Year, month, 1);

                    return new TransactionTimePeriodResponse(
                        item?.Total ?? 0,
                        date.ToString("MMM", CultureInfo.InvariantCulture)
                    );
                })
            .ToList();
    }
}
