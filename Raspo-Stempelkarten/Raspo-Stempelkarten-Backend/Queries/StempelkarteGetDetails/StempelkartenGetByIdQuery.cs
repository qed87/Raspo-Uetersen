using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StempelkarteGetDetails;

public record StempelkartenGetByIdQuery(string Team, string Season, Guid Id) 
    : IRequest<StempelkartenGetByIdQuery, Result<StempelkartenReadDetailsDto>>;