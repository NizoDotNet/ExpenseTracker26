using FluentValidation;

namespace ExpenseTracker.Application.Transactions.Requests;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransationRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(c => c.Name)
            .MinimumLength(1)
            .MaximumLength(50);

        RuleFor(c => c.Description)
            .MaximumLength(255);


    }
}