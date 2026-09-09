using FluentValidation;
using Guess43.Application.Authentication.Contracts;
using Guess43.Application.Games.Contracts;
using Guess43.Application.Users.Contracts;
using Guess43.Domain.Games;

namespace Guess43.Application.Validation;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.DisplayName).NotEmpty().Length(2, 50);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[A-Za-z]").WithMessage("Password must contain a letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().Length(2, 50);
    }
}

public sealed class SubmitGuessRequestValidator : AbstractValidator<SubmitGuessRequest>
{
    public SubmitGuessRequestValidator()
    {
        RuleFor(x => x.Guess)
            .InclusiveBetween(GameLimits.MinNumber, GameLimits.MaxNumber)
            .WithMessage($"Guess must be between {GameLimits.MinNumber} and {GameLimits.MaxNumber}.");
    }
}
