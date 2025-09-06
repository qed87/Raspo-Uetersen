using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteCreate;

public record StempelkartenCreateCommand(StempelkartenCreateDto Dto) 
    : IRequest<StempelkartenCreateCommand, Task<Result<StempelkartenCreateResponse>>>;