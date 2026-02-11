using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Commands.Shared.Interfaces;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.GetTeam;

/// <summary>
/// Gets the team
/// </summary>
[UsedImplicitly]
public record GetTeamQuery(string Team) : IRequest<GetTeamQuery, Task<Result<TeamDetailedReadDto?>>>, ITeamQuery;
