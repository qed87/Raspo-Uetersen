using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Commands.AddCoach;

/// <inheritdoc />
[UsedImplicitly]
public class AddCoachCommandHandler(IServiceProvider serviceProvider) 
    : CommandHandlerBase<AddCoachCommand, Unit>(serviceProvider)
{
    /// <inheritdoc />
    protected override async Task<Result<Unit>> ApplyCommandToModel(AddCoachCommand command, ITeamAggregate model)
    {
        var result = await model.AddCoachAsync(command.Email);
        return result;
    }
}