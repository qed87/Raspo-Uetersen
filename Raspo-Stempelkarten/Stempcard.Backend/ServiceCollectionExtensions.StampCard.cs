using DispatchR.Abstractions.Send;
using DispatchR.Abstractions.Stream;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;
using Raspo_Stempelkarten_Backend.Commands.CreateStampCard;
using Raspo_Stempelkarten_Backend.Commands.CreateTeamStampCardsForAccountingYear;
using Raspo_Stempelkarten_Backend.Commands.DeleteStampCard;
using Raspo_Stempelkarten_Backend.Commands.DeleteTeam;
using Raspo_Stempelkarten_Backend.Commands.EraseStamp;
using Raspo_Stempelkarten_Backend.Commands.StampStampCard;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.GetCompletedStampCardsQuery;
using Raspo_Stempelkarten_Backend.Queries.GetIncompletedStampCardsQuery;
using Raspo_Stempelkarten_Backend.Queries.GetStampCard;
using Raspo_Stempelkarten_Backend.Queries.GetStampCardDetails;
using Raspo_Stempelkarten_Backend.Queries.ListStampCards;
using Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;

namespace Raspo_Stempelkarten_Backend;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddStampCardCommands(this IServiceCollection services)
    {
        // Commands/Queries Stamp Card
        services.AddScoped<IRequestHandler<CreateTeamStampCardsForAccountingYears, Task<Result<CreateTeamStampCardsForAccountingYearsResponse>>>, 
            CreateTeamStampCardsForAccountingYearsRequestHandler>();
        services.AddScoped<IRequestHandler<StampStampCard, Task<Result<StampStampCardResponse>>>, StampStampCardRequestHandler>();
        services.AddScoped<IRequestHandler<EraseStamp, Task<Result<EraseStampResponse>>>, EraseStampRequestHandler>();
        services.AddScoped<IRequestHandler<CreateStampCard, Task<Result<CreateStampCardResponse>>>, CreateStampCardRequestHandler>();
        services.AddScoped<IRequestHandler<DeleteStampCard, Task<Result<DeleteStampCardResponse>>>, DeleteStampCardRequestHandler>();
        services.AddScoped<IRequestHandler<GetStampCardDetailsQuery, Task<StampCardReadDetailsDto?>>, GetStampCardDetailsQueryHandler>();
        services.AddScoped<IRequestHandler<GetStampCardQuery, Task<StampCardReadDto?>>, GetStampCardQueryHandler>();
        services.AddScoped<IStreamRequestHandler<ListStampCardsQuery, StampCardReadDto>, ListStampCardQueryHandler>();
        services.AddScoped<IRequestHandler<GetCompletedStampCardsQuery, Task<List<StampCardReadDetailsDto>?>>, GetCompletedStampCardsQueryHandler>();
        services.AddScoped<IRequestHandler<GetIncompletedStampCardsQuery, Task<List<StampCardReadDetailsDto>?>>, GetIncompletedStampCardsQueryHandler>();

        return services;
    }
}