using ExpenseTracker.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastracture.Configurations;

public class TransactionCategoryConfiguration : IEntityTypeConfiguration<TransactionCategory>
{
    public void Configure(EntityTypeBuilder<TransactionCategory> builder)
    {
        List<TransactionCategory> transactionCategories = new()
        {
            TransactionCategory.Create(1, "Food"),
            TransactionCategory.Create(2, "Joy"),
            TransactionCategory.Create(3, "Electronics"),
            TransactionCategory.Create(4, "For Home"),
            TransactionCategory.Create(5, "Transport"),
            TransactionCategory.Create(6, "Salary"),
            TransactionCategory.Create(7, "Other"),
        };
        builder.Property(c => c.Name)
            .HasMaxLength(50);

        builder.HasData(transactionCategories);
    }
}