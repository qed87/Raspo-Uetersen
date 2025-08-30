using FluentResults;
using JetBrains.Annotations;
using KurrentDB.Client;
using LiteBus.Commands.Abstractions;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Commands.StempelkarteCreate;
using Raspo_Stempelkarten_Backend.Commands.StempelkarteDelete;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteStamp;

[UsedImplicitly]
public class StempelkarteStampCommandHandler(
    KurrentDBClient kurrentDbClient, 
    IHttpContextAccessor contextAccessor, 
    IStempelkartenModelLoader modelLoader)
    : ICommandHandler<StempelkartenStampCommand, Result<StempelkartenStampResponse>>
{
    public Task<Result<StempelkartenStampResponse>> HandleAsync(StempelkartenStampCommand message, CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }
}