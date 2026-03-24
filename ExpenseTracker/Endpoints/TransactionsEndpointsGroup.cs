using ExpenseTracker.Application.Shared;
using ExpenseTracker.Application.Shared.Enums;
using ExpenseTracker.Application.Transactions.Requests;
using ExpenseTracker.Application.Transactions.Responses;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace ExpenseTracker.Endpoints;

public static class TransactionsEndpointsGroup
{
    extension(RouteGroupBuilder route)
    {
        public RouteGroupBuilder MapTransactionsEndpoints()
        {
            route.MapGet("/", GetTransactions);

            route.MapGet("/categories", async (TransactionService transactionService) =>
            {
                return await transactionService.GetTransactionCategoriesAsync();
            });

            route.MapGet("/time-period", GetTransactionsByTimePeriod);
            route.MapGet("/income-expense", GetTransactionIncomeExpense);

            route.MapPost("/", InsertTransaction);
            return route;
        }
    }

    internal static async Task<Results<Ok<PagedResult<TransactionResponse>>, BadRequest>> GetTransactions(int page, int? pageSize, HttpContext ctx, TransactionService transactionService, CancellationToken cancellationToken)
    {
        pageSize ??= 5;
        Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

        if (userId == default)
        {
            return TypedResults.BadRequest();
        }

        var res = await transactionService.GetAllAsyncWithPagination(userId, page, (int)pageSize, cancellationToken);

        return TypedResults.Ok(res);
    }
    internal static async Task<Results<Ok<List<TransactionTimePeriodResponse>>, BadRequest, NotFound>> GetTransactionsByTimePeriod(TimePeriod timePeriod, bool? isIncome, DateTimeOffset? dateTime, HttpContext ctx, UserService userService, TransactionService transactionService, CancellationToken cancellationToken)
    {
        dateTime = dateTime is null ? DateTimeOffset.UtcNow : ((DateTimeOffset)dateTime).ToUniversalTime();
        Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

        if (userId == default)
        {
            return TypedResults.BadRequest();
        }

        Guid? balanceId = await userService.GetUserBalanceId(userId);

        if (balanceId == null)
        {
            return TypedResults.NotFound();
        }

        var res = await transactionService.GetTransactionTimePeriodResponsesAsync((Guid)balanceId, timePeriod, isIncome, dateTime, cancellationToken);

        return TypedResults.Ok(res);
    }
    internal static async Task<Results<Created, ValidationProblem, BadRequest, NotFound>> InsertTransaction(CreateTransationRequest createTransation, TransactionService transactionService, HttpContext ctx, UserService userService, CancellationToken cancellationToken)
    {
        Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

        if (userId == default)
        {
            return TypedResults.BadRequest();
        }

        Guid? balanceId = await userService.GetUserBalanceId(userId);

        if (balanceId == null)
        {
            return TypedResults.NotFound();
        }
        var res = await transactionService.InsertAsync((Guid)balanceId, createTransation, cancellationToken);
        if (!res.IsSuccess)
        {
            return TypedResults.ValidationProblem(res.Errors);
        }

        return TypedResults.Created();
    }
    internal static async Task<Results<Ok<TransactionIncomeExpenseResponse>, BadRequest, NotFound>> GetTransactionIncomeExpense(TimePeriod timePeriod, DateTimeOffset? dateTime, HttpContext ctx, UserService userService, TransactionService transactionService, CancellationToken cancellationToken)
    {
        dateTime = dateTime is null ? DateTimeOffset.UtcNow : ((DateTimeOffset)dateTime).ToUniversalTime();
        Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

        if (userId == default)
        {
            return TypedResults.BadRequest();
        }

        Guid? balanceId = await userService.GetUserBalanceId(userId);

        if (balanceId == null)
        {
            return TypedResults.NotFound();
        }

        var income = await transactionService.GetTransactionTimePeriodResponsesAsync((Guid)balanceId, timePeriod, true, dateTime, cancellationToken);
        var expense = await transactionService.GetTransactionTimePeriodResponsesAsync((Guid)balanceId, timePeriod, false, dateTime, cancellationToken);

        var res = new TransactionIncomeExpenseResponse()
        {
            Labels = income.Select(c => c.Label).ToList(),
            Income = income.Select(c => new TransactionExpenseAmount(c.Amount)).ToList(),
            Expense = expense.Select(c => new TransactionExpenseAmount(c.Amount)).ToList(),
        };

        return TypedResults.Ok(res);
    }
}
