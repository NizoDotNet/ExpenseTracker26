using ExpenseTracker.Domain.Balances;
using ExpenseTracker.Domain.Transactions;
using ExpenseTracker.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastracture;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Balance> Balances { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionType> TransactionTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
    }
}
