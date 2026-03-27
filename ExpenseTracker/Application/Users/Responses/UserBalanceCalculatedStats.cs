namespace ExpenseTracker.Application.Users.Responses;

public record UserBalanceCalculatedStats(decimal Balance, decimal BalanceChange, decimal Income, decimal Expense, decimal SavingRate);
