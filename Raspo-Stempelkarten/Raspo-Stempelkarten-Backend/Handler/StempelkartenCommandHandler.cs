using LiteBus.Commands.Abstractions;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Handler;

public class StempelkartenCommandHandler : ICommandHandler<StempelkartenCreateCommand>
{
    public Task HandleAsync(
        StempelkartenCreateCommand message, 
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class StempelkartenCreateCommand : ICommand
{
    
}