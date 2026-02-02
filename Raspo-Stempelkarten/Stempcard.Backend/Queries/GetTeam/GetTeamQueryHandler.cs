using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Queries.GetTeam;

/// <inheritdoc />
public class GetTeamQueryHandler(IServiceProvider serviceProvider) 
    : QueryHandlerBase<GetTeamQuery, TeamDetailedReadDto?>(serviceProvider)
{
    /// <inheritdoc />
    protected override Task<TeamDetailedReadDto?> GetResult(ITeamAggregate model, GetTeamQuery request)
    {
        return Task.FromResult(new TeamDetailedReadDto(
            model.Id, 
            model.Club, 
            model.Name, 
            model.CreatedBy, 
            model.CreatedOn, 
            model.Coaches.Select(coach => coach.Email).ToList(), 
            model.Version ?? 0))!;
    }
}