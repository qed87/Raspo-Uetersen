using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.StampCardStamp;

public record StampCardStampCommand(string Team, string Season, Guid StampCardId, string? Reason) 
    : IRequest<StampCardStampCommand, Task<Result<StampCardStampResponse>>>;