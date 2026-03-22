using ExpenseTracker.Domain.Balances;
using ExpenseTracker.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastracture.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.Password)
            .IsRequired();

        builder.HasOne<Balance>()
            .WithOne()
            .HasForeignKey<User>(c => c.BalanceId)
            .HasForeignKey<Balance>(c => c.UserId);
    }
}
