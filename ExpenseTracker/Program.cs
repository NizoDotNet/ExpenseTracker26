using ExpenseTracker.Application;
using ExpenseTracker.Endpoints;
using ExpenseTracker.Infrastracture;
using ExpenseTracker.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.RegisterApplication();
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(c =>
{
    c.DefaultScheme = "cookie";
})
    .AddCookie("cookie");

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/auth")
    .MapAuthEndpoints()
    .WithTags("Auth");

app.MapGroup("/transactions")
    .MapTransactionsEndpoints()
    .RequireAuthorization()
    .WithTags("Transactions");
app.Run();

