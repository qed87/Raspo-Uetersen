using FluentValidation;
using StampCard.Backend.Commands.UpdatePlayer;

namespace StampCard.Backend.Validators;

/// <inheritdoc />
public class UpdatePlayerCommandValidator : AbstractValidator<UpdatePlayerCommand>
{
    /// <inheritdoc />
    public UpdatePlayerCommandValidator()
    {
        RuleFor(command => command.FirstName).NotEmpty().MinimumLength(2)
            .WithMessage("Vorname muss mindestens aus zwei Zeichen bestehen.");
        RuleFor(command => command.LastName).NotEmpty().MinimumLength(2)
            .WithMessage("Nachname muss mindestens aus zwei Zeichen bestehen.");
        RuleFor(command => command.Birthdate).LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-120)))
            .WithMessage("Geburtsdatum darf nicht in der Zukunft liegen oder weiter als 120 Jahre zurück liegen!");
        RuleFor(command => command.Birthplace).NotEmpty().MinimumLength(2)
            .WithMessage("Geburtsort darf nicht leer sein.");
        RuleFor(command => command.Team).NotEmpty().Matches("team-[0-9a-fA-F]{32}")
            .WithMessage("Ungültiger Team identifier.");
        RuleFor(command => command.ConcurrencyToken).GreaterThan((ulong) 0).WithMessage("ConcurrencyToken muss gesetzt sein!");
    }
}