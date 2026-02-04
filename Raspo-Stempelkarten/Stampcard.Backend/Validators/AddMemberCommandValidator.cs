using FluentValidation;
using StampCard.Backend.Commands.AddPlayer;

namespace StampCard.Backend.Validators;

/// <summary>
/// Validates add member commands.
/// </summary>
public class AddMemberCommandValidator : AbstractValidator<AddPlayerCommand>
{
    /// <summary>
    /// The validation rules of the validator.
    /// </summary>
    public AddMemberCommandValidator()
    {
        RuleFor(command => command.FirstName).NotEmpty().MinimumLength(2)
            .WithMessage("Vorname muss mindestens aus zwei Zeichen bestehen.");
        RuleFor(command => command.LastName).NotEmpty().MinimumLength(2)
            .WithMessage("Nachname muss mindestens aus zwei Zeichen bestehen.");
        RuleFor(command => command.Birthdate).LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Geburtsdatum darf nicht in der Zukunft liegen.");
        RuleFor(command => command.Birthplace).NotEmpty().MinimumLength(2)
            .WithMessage("Geburtsort darf nicht leer sein.");
        RuleFor(command => command.Team).NotEmpty().Matches("team-[0-9a-fA-F]{32}")
            .WithMessage("Ungültiger Team identifier.");
    }
}