using FluentValidation;

namespace ExpenseTracker.Application.Users.Requests;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(c => c.Email)
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(c => c.Password)
            .MinimumLength(5);

        RuleFor(c => c.Username)
            .MaximumLength(255);

    }
}