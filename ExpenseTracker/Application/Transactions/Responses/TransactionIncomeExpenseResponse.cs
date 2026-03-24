using Microsoft.Extensions.Primitives;

namespace ExpenseTracker.Application.Transactions.Responses;

public record TransactionIncomeExpenseResponse
{
    public List<string> Labels { get; set; } = [];
    public List<TransactionExpenseAmount> Expense { get; set; } = [];
    public List<TransactionExpenseAmount> Income { get; set; } = [];
}

public record TransactionExpenseAmount(decimal Amount);
