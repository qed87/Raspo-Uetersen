using FluentResults;
using LiteBus.Commands.Abstractions;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteStamp;

public record StempelkartenStampCommand(string Team, string Season, Guid StempelkartenId, string? Reason) 
    : ICommand<Result<StempelkartenStampResponse>>;