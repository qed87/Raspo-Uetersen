using FluentValidation;
using JetBrains.Annotations;
using StampCard.Backend.Commands.CreateTeamStampCardsForAccountingYear;

namespace StampCard.Backend.Validators;

/// <summary>
/// Validator for <see cref="CreateTeamStampCardsForAccountingYearsCommand" />.
/// </summary>
[UsedImplicitly]
public class CreateTeamStampCardsForAccountingYearsCommandValidator
: AbstractValidator<CreateTeamStampCardsForAccountingYearsCommand>
{
    /// <summary>
    /// Validation rules for <see cref="CreateTeamStampCardsForAccountingYearsCommand" />.
    /// </summary>
    public CreateTeamStampCardsForAccountingYearsCommandValidator()
    {
        RuleFor(command => command.AccountingYear).GreaterThanOrEqualTo(DateTime.Now.Year - 20)
            .LessThanOrEqualTo(DateTime.Now.Year)
            .WithMessage("Ungültiges Buchungsjahr. Darf nicht mehr als 20 Jahre in der Vergangenheit liegen und in der Zukunft liegen.");
    }
}