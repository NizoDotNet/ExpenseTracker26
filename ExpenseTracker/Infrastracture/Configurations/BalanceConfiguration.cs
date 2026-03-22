using ExpenseTracker.Domain.Balances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastracture.Configurations;

public class BalanceConfiguration : IEntityTypeConfiguration<Balance>
{
    public void Configure(EntityTypeBuilder<Balance> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(c => c.UserId);
    }
}
