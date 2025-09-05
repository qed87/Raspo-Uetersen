using Mapster;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Mappings;

public class DefaultRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<StampCard, StempelkartenReadDetailsDto>()
            .Map(dest => dest.ActualStamps, src => src.GetStamps().Count());
    }
}