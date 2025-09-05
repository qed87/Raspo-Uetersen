using FluentResults;
using LiteBus.Commands.Abstractions;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteCreate;

public record StempelkartenCreateCommand(StempelkartenCreateDto Dto) 
    : ICommand<Result<StempelkartenCreateResponse>>;