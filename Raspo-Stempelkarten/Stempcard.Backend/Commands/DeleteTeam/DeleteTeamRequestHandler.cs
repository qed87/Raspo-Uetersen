using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;
using Raspo_Stempelkarten_Backend.Commands.CreateStampCard;
using Raspo_Stempelkarten_Backend.Core;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.DeleteTeam;

/// <summary>
/// Deletes a team from the club repository.
/// </summary>
[UsedImplicitly]
public class DeleteTeamRequestHandler(IServiceProvider serviceProvider, ITeamService teamService) 
    : IRequestHandler<DeleteTeamRequest, Task<Result>>
{
    /// <inheritdoc />
    public async Task<Result> Handle(DeleteTeamRequest request, CancellationToken cancellationToken)
    {
        var teams = await teamService.ListTeamsAsync(cancellationToken);
        var found = teams?.SingleOrDefault(teamDto => teamDto.Id == request.Id);
        if(found == null) return Result.Fail("Team nicht gefunden!");
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.Id);
        var result = await model.DeleteTeamAsync();
        if (!result.IsSuccess) return result.ToResult();
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IEventStorage>();
        await storage.StoreAsync(request.Id, model.Version, changes, cancellationToken);
        return Result.Ok();
    }
}