using ExpenseTracker.Application.Transactions.Responses;
using ExpenseTracker.Infrastracture;

namespace ExpenseTracker.Application.Transactions.Helpers.GroupByTransactionsByCategory;

public interface IGroupByTransactionsByCategory
{
    Task<List<TransactionExpenseByCategoryResponse>> Handle(DatabaseContext dbContext, Guid balanceId, DateTimeOffset dateTime, CancellationToken cancellationToken = default);

}
