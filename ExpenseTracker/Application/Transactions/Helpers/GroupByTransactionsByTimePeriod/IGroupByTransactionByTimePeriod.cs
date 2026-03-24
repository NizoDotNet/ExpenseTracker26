using ExpenseTracker.Application.Transactions.Responses;
using ExpenseTracker.Infrastracture;

namespace ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByTimePeriod;

public interface IGroupByTransactionByTimePeriod
{
    Task<List<TransactionTimePeriodResponse>> Handle(DatabaseContext dbContext, Guid balanceId, DateTimeOffset dateTime, bool? isIncome = null, CancellationToken cancellationToken = default);
}
