using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.StampCardCreate;

public record StampCardCreateCommand(StampCardCreateDto Dto) 
    : IRequest<StampCardCreateCommand, Task<Result<StampCardCreateResponse>>>;