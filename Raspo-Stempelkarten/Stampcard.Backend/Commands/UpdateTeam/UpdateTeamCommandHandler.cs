using FluentResults;
using JetBrains.Annotations;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;
using StampCard.Backend.Services;

namespace StampCard.Backend.Commands.UpdateTeam;

/// <inheritdoc />
[UsedImplicitly]
public class UpdateTeamCommandHandler(
    IServiceProvider serviceProvider,
    ITeamService teamService,
    ILogger<UpdateTeamCommandHandler> logger) 
    : CommandHandlerBase<UpdateTeamCommand, ulong>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override async Task ApplyCommandToModelAsync(ICommandExecutionContext context)
    {
        var teams = await teamService.ListTeamsAsync();
        if (teams.Any(team => team.Name == context.Command.Name))
            context.SetResult(Result.Fail("Team mit demselben Namen existiert bereits."));
        logger.LogInformation("Update team name for team '{Team}'.", context.Command.Team);
        var result = await context.Model.UpdateAsync(context.Command.Name);
        context.SetResult(result);
    }

    /// <inheritdoc />
    protected override Task PrepareResultAsync(ICommandExecutionContext context)
    {
        context.SetResult(Result.Ok(CommitVersion ?? 0));
        return Task.CompletedTask;
    }
}