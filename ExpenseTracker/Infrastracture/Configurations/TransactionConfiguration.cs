using ExpenseTracker.Domain.Balances;
using ExpenseTracker.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastracture.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(50);

        builder.Property(c => c.Description)
            .IsRequired(false)
            .HasMaxLength(255);

        builder.Property(c => c.Amount)
            .HasDefaultValue(0.00);

        builder.Ignore(c => c.Events);

        builder.HasOne<Balance>();

        builder.HasOne<TransactionCategory>();
    }
}
