using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Queries.GetMember;

/// <inheritdoc />
public class GetMemberQueryHandler(IServiceProvider serviceProvider) 
    : QueryHandlerBase<GetMemberQuery, MemberReadDto?>(serviceProvider)
{
    /// <inheritdoc />
    protected override Task<MemberReadDto?> GetResult(ITeamAggregate model, GetMemberQuery request)
    {
        var member = model.Members.SingleOrDefault(mem => mem.Id == request.Id && !mem.Deleted);
        if (member is null) return null!;
        var memberReadDto = new MemberReadDto(member.Id, member.FirstName, member.LastName, member.Birthdate, member.Birthplace);
        return Task.FromResult(memberReadDto)!;
    }
}