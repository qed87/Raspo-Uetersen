using DispatchR.Abstractions.Send;
using DispatchR.Abstractions.Stream;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.ListPlayers;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.ListCoach;

[UsedImplicitly]
public class ListCoachQuery(string id, string issuerName) : IRequest<ListCoachQuery, Task<List<string>>>
{
    public string TeamId { get; set; } = id;
    
    public string IssuedBy { get; set; } = issuerName;
}