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

            route.MapGet("/login", async (LoginUserRequest loginUserRequest, UserService userService, CancellationToken cancellationToken) =>
            {
                var user = await userService.LoginUser(loginUserRequest.Email, loginUserRequest.Password, cancellationToken);
                if(user == null)
                {
                    return Results.BadRequest();
                }

                return Results.SignIn(new ClaimsPrincipal(
                    [new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())], "cookie")]
                    )
                );
            });
            return route; 
        }
    }
}
