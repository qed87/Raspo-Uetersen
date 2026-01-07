using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.DeleteStampCard;

public record DeleteStampCard(Guid id, string streamId) : IRequest<DeleteStampCard, Task<Result<DeleteStampCardResponse>>>;