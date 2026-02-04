using FluentValidation;
using JetBrains.Annotations;
using StampCard.Backend.Commands.AddTeam;

namespace StampCard.Backend.Validators;

/// <summary>
/// Update team request validator.
/// </summary>
[UsedImplicitly]
public class AddTeamRequestValidator : AbstractValidator<AddTeamCommand>
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