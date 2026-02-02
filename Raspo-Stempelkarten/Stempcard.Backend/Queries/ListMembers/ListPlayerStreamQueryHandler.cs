using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Queries.ListMembers;

/// <inheritdoc />
[UsedImplicitly]
public class ListPlayerStreamQueryHandler(IServiceProvider serviceProvider) : IStreamRequestHandler<ListMembersQuery, MemberReadDto>
{
    /// <inheritdoc />
    public async IAsyncEnumerable<MemberReadDto> Handle(ListMembersQuery request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.Team);
        foreach (var member in model.Members.Where(player => !player.Deleted))
        {
            var memberReadDto = new MemberReadDto(member.Id, member.FirstName, member.LastName, 
                member.Birthdate, member.Birthplace);
            yield return memberReadDto;
        }
    }
}