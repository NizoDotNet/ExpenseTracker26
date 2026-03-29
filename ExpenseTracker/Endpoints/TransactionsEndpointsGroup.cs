using ExpenseTracker.Application.Shared;
using ExpenseTracker.Application.Shared.Enums;
using ExpenseTracker.Application.Transactions.Requests;
using ExpenseTracker.Application.Transactions.Responses;
using ExpenseTracker.Domain.Users;
using ExpenseTracker.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Endpoints;

public static class TransactionsEndpointsGroup
{
    extension(RouteGroupBuilder route)
    {
        public RouteGroupBuilder MapTransactionsEndpoints()
        {
            route.MapGet("/", Get);
            route.MapDelete("/{transactionId}", Delete);
            route.MapPatch("/{transactionId}", Update);
            route.MapGet("/by-category", GetByCategory);
            route.MapGet("/categories", async (TransactionService transactionService) =>
            {
                return await transactionService.GetTransactionCategoriesAsync();
            });
            route.MapGet("/calculated-balance", async (TransactionService transactionService, HttpContext ctx) =>
            {
                Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

                if (userId == default)
                {
                    return Results.Unauthorized();
                }

                return Results.Ok(await transactionService.GetUsersCalculatedBalanceStats(userId));

            });

            route.MapGet("/time-period", GetByTimePeriod);
            route.MapGet("/income-expense", GetIncomeExpense);

            route.MapPost("/", Insert);
            return route;
        }
    }

    internal static async Task<Results<Ok<PagedResult<TransactionResponse>>, UnauthorizedHttpResult>> Get(
        int page,
        int? pageSize,
        HttpContext ctx,
        TransactionService transactionService,
        CancellationToken cancellationToken)
    {
        pageSize ??= 5;
        Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

        if (userId == default)
        {
            return TypedResults.Unauthorized();
        }

        var res = await transactionService.GetAllAsyncWithPagination(userId, page, (int)pageSize, cancellationToken);

        return TypedResults.Ok(res);
    }
    internal static async Task<Results<Ok, UnauthorizedHttpResult, BadRequest>> Delete(
        [FromRoute] Guid transactionId,
        TransactionService transactionService,
        HttpContext ctx,
        CancellationToken cancellationToken)
    {
        Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

        if (userId == default)
        {
            return TypedResults.Unauthorized();
        }
        var res = await transactionService.DeleteAsync(transactionId, userId, cancellationToken);
        if(!res.IsSuccess)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok();
    }
    internal static async Task<Results<Ok, ValidationProblem, BadRequest, UnauthorizedHttpResult>> Update(
        [FromRoute] Guid transactionId,
        UpdateTransactionRequest updateTransactionRequest,
        TransactionService transactionService,
        HttpContext ctx,
        CancellationToken cancellationToken)
    {
        Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

        if (userId == default)
        {
            return TypedResults.Unauthorized();
        }
        var res = await transactionService
            .UpdateAsync(transactionId, userId, updateTransactionRequest, cancellationToken);

        if(!res.IsSuccess)
        {
            if(res.Errors.Any())
            {
                return TypedResults.ValidationProblem(res.Errors);
            }
            return TypedResults.BadRequest();
        }

        return TypedResults.Ok();
    }
    internal static async Task<Results<Ok<List<TransactionExpenseByCategoryResponse>>, UnauthorizedHttpResult>> GetByCategory(
        TimePeriod timePeriod,
        DateTimeOffset? date,
        TransactionService transactionService,
        HttpContext ctx,
        CancellationToken cancellationToken) 
    {
        Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

        if (userId == default)
        {
            return TypedResults.Unauthorized();
        }
        date ??= DateTimeOffset.UtcNow;


        var transactions = await transactionService.GetExpensesByCategory(userId, timePeriod, date, cancellationToken);
        return TypedResults.Ok(transactions);
    }
    internal static async Task<Results<Ok<List<TransactionTimePeriodResponse>>, UnauthorizedHttpResult, NotFound>> GetByTimePeriod(
        TimePeriod timePeriod,
        bool? isIncome,
        DateTimeOffset? dateTime,
        HttpContext ctx,
        UserService userService,
        TransactionService transactionService,
        CancellationToken cancellationToken)
    {
        dateTime = dateTime is null ? DateTimeOffset.UtcNow : ((DateTimeOffset)dateTime).ToUniversalTime();
        Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

        if (userId == default)
        {
            return TypedResults.Unauthorized();
        }

        Guid? balanceId = await userService.GetUserBalanceId(userId);

        if (balanceId == null)
        {
            return TypedResults.NotFound();
        }

        var res = await transactionService.GetTransactionTimePeriodResponsesAsync((Guid)balanceId, timePeriod, isIncome, dateTime, cancellationToken);

        return TypedResults.Ok(res);
    }
    internal static async Task<Results<Created, ValidationProblem, UnauthorizedHttpResult, NotFound>> Insert(
        CreateTransationRequest createTransation,
        TransactionService transactionService,
        HttpContext ctx,
        UserService userService,
        CancellationToken cancellationToken)
    {
        Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

        if (userId == default)
        {
            return TypedResults.Unauthorized();
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

        return TypedResults.Created(res.Value!.Id.ToString());
    }
    internal static async Task<Results<Ok<TransactionIncomeExpenseResponse>, UnauthorizedHttpResult, NotFound>> GetIncomeExpense(
        TimePeriod timePeriod,
        DateTimeOffset? dateTime,
        HttpContext ctx,
        UserService userService,
        TransactionService transactionService,
        CancellationToken cancellationToken)
    {
        dateTime = dateTime is null ? DateTimeOffset.UtcNow : ((DateTimeOffset)dateTime).ToUniversalTime();
        Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

        if (userId == default)
        {
            return TypedResults.Unauthorized();
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
