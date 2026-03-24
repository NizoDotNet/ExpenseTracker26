using ExpenseTracker.Application.Shared.Enums;
using ExpenseTracker.Application.Transactions.Requests;
using ExpenseTracker.Services;
using System.Security.Claims;

namespace ExpenseTracker.Endpoints;

public static class TransactionsEndpointsGroup 
{
    extension(RouteGroupBuilder route)
    {
        public RouteGroupBuilder MapTransactionsEndpoints()
        {
            route.MapGet("/", async (int page, int? pageSize, HttpContext ctx, TransactionService transactionService, CancellationToken cancellationToken) =>
            {
                pageSize ??= 5;
                Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

                if (userId == default)
                {
                    return TypedResults.BadRequest();
                }

                var res = await transactionService.GetAllAsyncWithPagination(userId, page, (int)pageSize, cancellationToken);

                return Results.Ok(res);
            });

            route.MapGet("/categories", async (TransactionService transactionService) =>
            {
                return await transactionService.GetTransactionCategoriesAsync();
            });

            route.MapGet("/time-period", async (TimePeriod timePeriod, bool? isIncome, DateTimeOffset? dateTime, HttpContext ctx, UserService userService, TransactionService transactionService, CancellationToken cancellationToken) =>
            {
                dateTime = dateTime is null ? DateTimeOffset.UtcNow : ((DateTimeOffset)dateTime).ToUniversalTime();
                Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

                if (userId == default)
                {
                    return TypedResults.BadRequest();
                }

                Guid? balanceId = await userService.GetUserBalanceId(userId);

                if(balanceId == null)
                {
                    return TypedResults.NotFound();
                }

                var res = await transactionService.GetTransactionTimePeriodResponsesAsync((Guid)balanceId, timePeriod, isIncome, dateTime, cancellationToken);

                return Results.Ok(res);
            });

            route.MapPost("/", async (CreateTransationRequest createTransation, TransactionService transactionService, HttpContext ctx, UserService userService, CancellationToken cancellationToken) =>
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
                if(!res.IsSuccess)
                {
                    return TypedResults.ValidationProblem(res.Errors);
                }

                return Results.Created();
            });
            return route;
        }
    }
}
