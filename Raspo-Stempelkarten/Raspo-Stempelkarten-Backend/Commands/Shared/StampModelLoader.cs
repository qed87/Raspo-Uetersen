using Castle.DynamicProxy;
using DispatchR;
using FluentResults;
using JetBrains.Annotations;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

[UsedImplicitly]
public class StampModelLoader(
    IMediator mediator,
    IProxyGenerator proxyGen,
    KurrentDBClient kurrentDbClient) 
    : IStampModelLoader
{
    public async Task<IStampModel> LoadModelAsync(string streamId)
    {
        var result = kurrentDbClient.ReadStreamAsync(
            Direction.Forwards,
            streamId,
            StreamPosition.Start);
        var replayer = new StampModelReplayer(streamId);
        ulong? streamRevision = null;
        if (await result.ReadState == ReadState.Ok)
        {
            await foreach (var resolvedEvent in result)
            {
                replayer.Replay(resolvedEvent);
                streamRevision = resolvedEvent.OriginalEventNumber.ToUInt64();
            }
        }

        var modelAggregate = replayer.GetModel();
        modelAggregate.Version = streamRevision;
        var proxyWithTarget = proxyGen.CreateInterfaceProxyWithTarget<IStampModel>(
            modelAggregate, 
            new StampModelChangeInterceptor(mediator));
        return proxyWithTarget;
    }
}

public class StampModelChangeInterceptor(IMediator mediator) : IInterceptor
{
    public async void Intercept(IInvocation invocation)
    {
        try
        {
            if (invocation.Method.Name == nameof(StampModel.AddPlayer))
            {
                invocation.Proceed();
                var resultCode = (Result<Guid>) invocation.ReturnValue!;
                if (resultCode.IsSuccess)
                {
                    var firstName = (string) invocation.Arguments[0]!; 
                    var surname = (string) invocation.Arguments[1]!;  
                    var birthdate = (DateOnly) invocation.Arguments[2]!;
                    await mediator.Publish(new PlayerAdded(resultCode.Value, firstName, surname, birthdate), 
                        cancellationToken: CancellationToken.None);
                }
            }
            else if (invocation.Method.Name == nameof(StampModel.DeletePlayer))
            {
                invocation.Proceed();
                var resultCode = (Result<Guid>) invocation.ReturnValue!;
                if (resultCode.IsSuccess)
                {
                    await mediator.Publish(new PlayerDeleted(resultCode.Value), 
                    cancellationToken: CancellationToken.None);
                }
            }
            else if (invocation.Method.Name == nameof(StampModel.AddStampCard))
            {
                invocation.Proceed();
                var stampCardResult = (Result<StampCard>) invocation.ReturnValue!;
                if (stampCardResult.IsSuccess)
                {
                    await mediator.Publish(
                        new StampCardAdded 
                        {
                            Id = stampCardResult.Value.Id,
                            AccountingYear = stampCardResult.Value.AccountingYear,
                            IssuedAt = stampCardResult.Value.IssuedAt,
                            IssuedTo = stampCardResult.Value.IssuedTo
                        },
                        cancellationToken: CancellationToken.None);
                }
            }
            else if (invocation.Method.Name == nameof(StampModel.AddStamp))
            {
                var stampCardId = (Guid) invocation.Arguments[0]!; 
                invocation.Proceed();
                var stampResult = (Result<Stamp>) invocation.ReturnValue!;
                if (stampResult.IsSuccess)
                {
                    await mediator.Publish(
                        new StampAdded 
                        {
                            Id = stampResult.Value.Id,
                            StampCardId = stampCardId,
                            Reason = stampResult.Value.Reason,
                            IssuedBy = stampResult.Value.IssuedBy,
                            IssuedAt = stampResult.Value.IssuedAt
                        },
                        cancellationToken: CancellationToken.None);
                }
            }
            else if (invocation.Method.Name == nameof(StampModel.EraseStamp))
            {
                var stampCardId = (Guid) invocation.Arguments[0]!;
                invocation.Proceed();
                var stampResult = (Result<Stamp>) invocation.ReturnValue!;
                if (stampResult.IsSuccess)
                {
                    await mediator.Publish(
                        new StampCardRemoved 
                        {
                            Id = stampResult.Value.Id,
                            StampCardId = stampCardId
                        },
                        cancellationToken: CancellationToken.None);
                }
            }
            else if (invocation.Method.Name == nameof(StampModel.DeleteStampCard))
            {
                invocation.Proceed();
                var stampCardResult = (Result<Guid>) invocation.ReturnValue!;
                if (stampCardResult.IsSuccess)
                {
                    await mediator.Publish(
                        new StampCardRemoved 
                        {
                            Id = stampCardResult.Value
                        },
                        cancellationToken: CancellationToken.None);
                }
            }
            else
            {
                invocation.Proceed();
            }
        }
        catch (Exception e)
        {
            throw; // TODO handle exception
        }
    }
}