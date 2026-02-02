using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

/// <summary>
/// This event is fired when a new member is added to the team.
/// </summary>
public record MemberAdded(Guid Id, string FirstName, string LastName, DateOnly Birthdate, string Birthplace, 
    string Issuer, DateTimeOffset IssuedOn) : INotification;