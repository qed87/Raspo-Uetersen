using System.Linq.Expressions;
using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;

/// <inheritdoc />
public class ListTeamsQueryHandler(ITeamService teamService, IHttpContextAccessor httpContextAccessor) 
    : IRequestHandler<ListTeamsQuery, Task<List<TeamReadDto>>>
{
    /// <inheritdoc />
    public async Task<List<TeamReadDto>> Handle(ListTeamsQuery request, CancellationToken cancellationToken)
    {
        var teams = await teamService.ListTeamsAsync(cancellationToken);
      Expression<Func<TeamReadDto, bool>> filter = dto => true;
        if (!httpContextAccessor.HttpContext!.User.IsInRole("manger"))
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