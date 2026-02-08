using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.GetTeam;

/// <inheritdoc />
public class GetTeamQueryHandler(IServiceProvider serviceProvider, ILogger<GetTeamQueryHandler> logger) 
    : QueryHandlerBase<GetTeamQuery, TeamDetailedReadDto?>(serviceProvider, logger)
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
            model.Deleted,
            model.Coaches.Select(coach => coach.Email).ToList(), 
            model.Version ?? 0))!;
    }
}