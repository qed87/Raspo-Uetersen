using FluentValidation;
using StampCard.Backend.Commands.AddCoach;

namespace StampCard.Backend.Validators;

/// <summary>
/// Validates <see cref="AddCoachCommand"/>
/// </summary>
public class AddCoachCommandValidator : AbstractValidator<AddCoachCommand>
{
    /// <summary>
    /// Validation rules of this validator.
    /// </summary>
    public AddCoachCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress().WithMessage("Invalid email address.");
    }
}