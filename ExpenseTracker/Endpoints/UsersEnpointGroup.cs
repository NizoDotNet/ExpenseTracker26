using ExpenseTracker.Application.Users.Requests;
using ExpenseTracker.Services;
using System.Security.Claims;

namespace ExpenseTracker.Endpoints;

public static class UsersEnpointGroup
{
    extension(RouteGroupBuilder route)
    {
        public RouteGroupBuilder MapAuthEndpoints()
        {
            route.MapPost("/register", async (CreateUserRequest createUserRequest,
                UserService userService,
                CancellationToken cancellationToken) =>
            {
                var res = await userService.RegisterAsync(createUserRequest, cancellationToken);
                if (res.IsSuccess)
                {
                    return Results.Ok();
                }

                return TypedResults.ValidationProblem(res.Errors);
            });

            route.MapPost("/login", async (LoginUserRequest loginUserRequest, UserService userService, CancellationToken cancellationToken) =>
            {
                var user = await userService.LoginUser(loginUserRequest.Email, loginUserRequest.Password, cancellationToken);
                if(user == null)
                {
                    return TypedResults.BadRequest();
                }

                return Results.SignIn(new ClaimsPrincipal(
                    [new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())], "cookie")]
                    )
                );
            });

            route.MapGet("/user", async (HttpContext ctx, UserService userService, CancellationToken cancellationToken) =>
            {
                Guid.TryParse(ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId);

                if(userId == default)
                {
                    return TypedResults.BadRequest();
                }

                var user = await userService.GetAsync(userId, false);
                if(user == null)
                {
                    return TypedResults.NotFound();
                }

                return Results.Ok(user);

            }).RequireAuthorization();

            route.MapPost("/logout", () => Results.SignOut());
            return route; 
        }
    }
}
