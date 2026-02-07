using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.GetIncompletedStampCardsQuery;

/// <summary>
/// Returns all stamp cards that are incompleted.
/// </summary>
/// <param name="Team"></param>
/// <param name="AccountingYear"></param>
public record GetIncompletedStampCardsQuery(string Team, int AccountingYear, int NumberOfRequiredStamps)
    : IRequest<GetIncompletedStampCardsQuery, Task<Result<List<StampCardReadDetailsDto>>>>, ITeamQuery;