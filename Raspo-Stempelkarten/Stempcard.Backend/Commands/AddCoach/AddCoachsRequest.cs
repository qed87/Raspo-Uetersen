using DispatchR.Abstractions.Send;
using FluentResults;

namespace Raspo_Stempelkarten_Backend.Commands.AddCoach;

/// <inheritdoc />
public class AddCoach(string team, string email, string issuer)
    : IRequest<AddCoach, Task<Result>>
{
    public string Team { get; } = team;
    public string Email { get; } = email;
    public string IssuedBy { get; set; } = issuer;
    
    public DateTimeOffset IssuedDate { get; set; } = DateTimeOffset.UtcNow;
}