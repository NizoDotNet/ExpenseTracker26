using FluentValidation;

namespace ExpenseTracker.Application.Transactions.Requests;

public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionRequestValidator()
    {
        RuleFor(c => c.Name)
            .MinimumLength(1)
            .MaximumLength(50);

        RuleFor(c => c.Description)
            .MaximumLength(255);


    }
}