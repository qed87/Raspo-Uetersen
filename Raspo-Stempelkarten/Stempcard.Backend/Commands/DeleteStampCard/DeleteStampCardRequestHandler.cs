using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.DeleteStampCard;

public class DeleteStampCardRequestHandler : IRequestHandler<DeleteStampCard, Task<Result<DeleteStampCardResponse>>>
{
    public Task<Result<DeleteStampCardResponse>> Handle(DeleteStampCard request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}