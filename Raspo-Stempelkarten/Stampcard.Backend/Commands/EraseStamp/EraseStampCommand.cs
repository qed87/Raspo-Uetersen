using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;

namespace StampCard.Backend.Commands.EraseStamp;

/// <summary>
/// Erases a stamp from the given stamp card. 
/// </summary>
public record EraseStampCommand(Guid StampCardId, Guid Id, string Team) 
    : IRequest<EraseStampCommand, Task<Result<Guid>>>, ITeamCommand;