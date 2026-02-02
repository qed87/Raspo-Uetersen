using System.Text.Json;
using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.UpdateTeam;

/// <inheritdoc />
[UsedImplicitly]
public class UpdateTeamCommandHandler(
    IServiceProvider serviceProvider) 
    : CommandHandlerBase<UpdateTeamCommand, ulong>(serviceProvider)
{
    /// <inheritdoc />
    protected override async Task<Result<ulong>> ApplyCommandToModel(UpdateTeamCommand command, ITeamAggregate model)
    {
        return await model.UpdateAsync(command.Name);
    }
}