using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.EraseStamp;

/// <summary>
/// Erases a stamp from the given stamp card. 
/// </summary>
public record EraseStampCommand(Guid StampCardId, Guid StampId, string Team) 
    : IRequest<EraseStampCommand, Task<Result<Guid>>>, ITeamCommand;