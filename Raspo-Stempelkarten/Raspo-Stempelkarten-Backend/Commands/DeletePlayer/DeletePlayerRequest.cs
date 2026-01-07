using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.AddPlayer;

namespace Raspo_Stempelkarten_Backend.Commands.DeletePlayer;

public class DeletePlayerRequest(Guid id, string team)
    : IRequest<DeletePlayerRequest, Task<Result<DeletePlayerResponse>>>
{
    public string Team { get; } = team;

    public Guid Id { get; set; } = id;

}