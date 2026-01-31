using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.RemoveCoach;

/// <inheritdoc />
public class RemoveCoach(string team, string email, string issuer)
    : IRequest<RemoveCoach, Task<Result>>
{
    public string Team { get; } = team;
    public string Email { get; } = email;
    public string Issuer { get; } = issuer;
}