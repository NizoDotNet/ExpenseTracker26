using ExpenseTracker.Application.Transactions.Responses;
using ExpenseTracker.Infrastracture;

namespace ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByTimePeriod;

public interface IGroupByTransactionByTimePeriod
{
    Task<List<TransactionTimePeriodResponse>> Handle(DatabaseContext dbContext, DateTimeOffset dateTime, CancellationToken cancellationToken = default);
}
