using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;
using Raspo_Stempelkarten_Backend.Commands.CreateStampCard;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.DeleteTeam;

/// <summary>
/// Deletes a team from the club repository.
/// </summary>
[UsedImplicitly]
public class DeleteTeamCommandHandler(IServiceProvider serviceProvider, ITeamService teamService) 
    : CommandHandlerBase<DeleteTeamCommand, string>(serviceProvider)
{
    /// <inheritdoc />
    protected override async Task<Result> BeforeCommandExecutionAsync(
        ITeamAggregate teamModel,
        DeleteTeamCommand command,
        IServiceProvider services)
    {
        var teams = await teamService.ListTeamsAsync(CancellationToken.None);
        var found = teams?.SingleOrDefault(teamDto => teamDto.Id == command.Team);
        return found == null ? Result.Fail("Team nicht gefunden!") : Result.Ok();
    }

    /// <inheritdoc />
    protected override async Task<Result<string>> ApplyCommandToModel(DeleteTeamCommand command, ITeamAggregate model)
    {
        return await model.DeleteTeamAsync();
    }
}