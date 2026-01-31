using FluentValidation;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;

namespace Raspo_Stempelkarten_Backend.Validators;

/// <summary>
/// Add team request validator.
/// </summary>
public class AddTeamRequestValidator : AbstractValidator<AddTeamRequest>
{
    /// <summary>
    /// Creates a new validator.
    /// </summary>
    public AddTeamRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().WithMessage("Name ist ein Pflichtparameter.");
        RuleFor(request => request.Club).NotEmpty().WithMessage("Club ist ein Pflichtparameter.");
    }
}