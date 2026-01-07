using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.EraseStamp;

public record EraseStamp(Guid StampCardId, Guid StampId, string Team) : IRequest<EraseStamp, Task<Result<EraseStampResponse>>>;