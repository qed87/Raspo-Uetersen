using FluentResults;
using JetBrains.Annotations;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;
using StampCard.Backend.Services;
using StampCard.Backend.Services.Interfaces;

namespace StampCard.Backend.Commands.DeleteTeam;

/// <summary>
/// Deletes a team from the club repository.
/// </summary>
[UsedImplicitly]
public class DeleteTeamCommandHandler(IServiceProvider serviceProvider, ITeamService teamService, ILogger<DeleteTeamCommandHandler> logger) 
    : CommandHandlerBase<DeleteTeamCommand, string>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override async Task PrepareCommandAsync(ICommandExecutionContext context)
    {
        await base.PrepareCommandAsync(context);
        var teams = await teamService.ListTeamsAsync(CancellationToken.None);
        var found = teams.SingleOrDefault(teamDto => teamDto.Id == context.Command.Team);
        if (found is null) context.SetResult(Result.Fail("Team nicht gefunden!"));
    }

    /// <inheritdoc />
    protected override async Task ApplyCommandToModelAsync(ICommandExecutionContext context)
    {
        logger.LogInformation("Delete team with Id '{StreamId}'.", context.Command.Team);
        var result = await context.Model.DeleteTeamAsync();
        context.SetResult(result);
    }
}