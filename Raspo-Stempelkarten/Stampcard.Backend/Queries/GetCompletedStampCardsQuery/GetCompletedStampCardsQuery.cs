using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Commands.Shared.Interfaces;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.GetCompletedStampCardsQuery;

/// <summary>
/// Returns all stamp cards that are completed.
/// </summary>
/// <param name="Team"></param>
/// <param name="AccountingYear"></param>
public record GetCompletedStampCardsQuery(string Team, int AccountingYear, int NumberOfRequiredStamps)
    : IRequest<GetCompletedStampCardsQuery, Task<Result<List<StampCardReadDetailsDto>>>>, ITeamQuery;