using FluentResults;
using LiteBus.Queries.Abstractions;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StempelkarteGetDetails;

public record StempelkartenGetByIdQuery(string Team, string Season, Guid Id) 
    : IQuery<Result<StempelkartenReadDetailsDto>>;