using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.GetPlayer;

public class GetPlayerQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<GetPlayersQuery, Task<PlayerReadDto?>>
{
    public async Task<PlayerReadDto?> Handle(GetPlayersQuery request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var stampModelLoader = serviceProvider.GetRequiredService<IStampModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.StreamId);
        var player = model.Players.SingleOrDefault(player => player.Id == request.Id && !player.Deleted);
        if (player is null) return null;
        var playerReadDto = new PlayerReadDto
        {
            Id = player.Id,
            FirstName = player.FirstName,
            Surname = player.Surname,
            BirthDate = player.Birthdate
        };
        
        return playerReadDto;
    }
}