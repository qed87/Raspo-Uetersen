using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.StampCardUpdate;

public record StampCardUpdateCommand(StampCardUpdateDto Dto) 
    : IRequest<StampCardUpdateCommand, Task<Result<StampCardUpdateResponse>>>;