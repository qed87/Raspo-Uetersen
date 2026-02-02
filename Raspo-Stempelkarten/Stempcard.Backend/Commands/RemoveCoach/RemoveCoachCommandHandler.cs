using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.RemoveCoach;

/// <inheritdoc />
[UsedImplicitly]
public class RemoveCoachCommandHandler(IServiceProvider serviceProvider) : 
    CommandHandlerBase<RemoveCoachCommand, Unit>(serviceProvider)
{
    /// <inheritdoc />
    protected override async Task<Result<Unit>> ApplyCommandToModel(RemoveCoachCommand command, ITeamAggregate model)
    {
        return await model.RemoveCoach(command.Email);
    }
}