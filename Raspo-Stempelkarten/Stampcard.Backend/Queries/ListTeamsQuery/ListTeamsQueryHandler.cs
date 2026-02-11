using System.Linq.Expressions;
using DispatchR.Abstractions.Send;
using StampCard.Backend.Services;
using StampCard.Backend.Services.Interfaces;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.ListTeamsQuery;

/// <inheritdoc />
public class ListTeamsQueryHandler(ITeamService teamService, IHttpContextAccessor httpContextAccessor) 
    : IRequestHandler<ListTeamsQuery, Task<List<TeamReadDto>>>
{
    /// <inheritdoc />
    public async Task<List<TeamReadDto>> Handle(ListTeamsQuery request, CancellationToken cancellationToken)
    {
        var teams = await teamService.ListTeamsAsync(cancellationToken);
        Expression<Func<TeamReadDto, bool>> filter = dto => true;
        if (!httpContextAccessor.HttpContext!.User.IsInRole("manager"))
        {
            filter = Expression.Lambda<Func<TeamReadDto, bool>>(
                Expression.AndAlso(
                    filter.Body,
                    Expression.Invoke((Expression<Func<TeamReadDto, bool>>) 
                        (dto => dto.Coaches.Contains(httpContextAccessor.HttpContext.User.Identity!.Name!)),
                        filter.Parameters[0])),
                    filter.Parameters[0]);
        }

        var predicate = filter.Compile();
        return teams.Where(predicate).ToList();
    }
}