using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.AddPlayer;

[UsedImplicitly]
public class AddPlayersRequestHandler(IStampModelLoader stampModelLoader) : IRequestHandler<AddPlayersRequest, Task<Result<AddPlayersResponse>>>
{
    public async Task<Result<AddPlayersResponse>> Handle(AddPlayersRequest request, CancellationToken cancellationToken)
    {
        var model = await stampModelLoader.LoadModelAsync(request.Team);
        var result = model.AddPlayer(request.FirstName, request.Surname, request.Birthdate);
        return Result.Ok(new AddPlayersResponse());
    }
}