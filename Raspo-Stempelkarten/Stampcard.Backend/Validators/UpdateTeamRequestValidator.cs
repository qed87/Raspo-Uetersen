using FluentValidation;
using JetBrains.Annotations;
using StampCard.Backend.Commands.UpdateTeam;

namespace StampCard.Backend.Validators;

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