using FluentValidation;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.UpdateTeam;

namespace Raspo_Stempelkarten_Backend.Validators;

/// <summary>
/// Update team request validator.
/// </summary>
[UsedImplicitly]
public class UpdateTeamRequestValidator : AbstractValidator<UpdateTeamCommand>
{
    /// <summary>
    /// Creates a new validator.
    /// </summary>
    public UpdateTeamRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MinimumLength(2).WithMessage("Name ist ein Pflichtparameter.");
        RuleFor(request => request.ConcurrencyToken).NotEmpty().WithMessage("ConcurrencyToken ist ein Pflichtparameter.");
    }
}