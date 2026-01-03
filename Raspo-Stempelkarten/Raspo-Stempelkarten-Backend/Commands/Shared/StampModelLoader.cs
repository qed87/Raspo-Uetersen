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