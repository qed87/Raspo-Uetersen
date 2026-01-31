using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Core;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.GetPlayer;

/// <inheritdoc />
public class GetPlayerQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<GetPlayersQuery, Task<PlayerReadDto?>>
{
    /// <inheritdoc />
    public async Task<PlayerReadDto?> Handle(GetPlayersQuery request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.StreamId);
        var player = model.Players.SingleOrDefault(player => player.Id == request.Id && !player.Deleted);
        if (player is null) return null;
        var playerReadDto = new PlayerReadDto
        {
            Id = player.Id,
            FirstName = player.FirstName,
            Surname = player.LastName,
            BirthDate = player.Birthdate
        };
        
        return playerReadDto;
    }
}