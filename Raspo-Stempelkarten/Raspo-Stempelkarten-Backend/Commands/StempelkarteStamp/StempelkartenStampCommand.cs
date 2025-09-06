using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteStamp;

public record StempelkartenStampCommand(string Team, string Season, Guid StempelkartenId, string? Reason) 
    : IRequest<StempelkartenStampCommand, Task<Result<StempelkartenStampResponse>>>;