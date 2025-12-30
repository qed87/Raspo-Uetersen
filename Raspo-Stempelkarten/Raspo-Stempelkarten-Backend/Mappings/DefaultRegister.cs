using Mapster;
using QRCoder;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Mappings;

public class DefaultRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<StampCard, StampCardReadDetailsDto>()
            .Map(dest => dest.Owners, src => src.GetOwners())
            .Map(dest => dest.Stamps, src => src.GetStamps())
            .Map(dest => dest.ActualStamps, src => src.GetStamps().Count());
        config.NewConfig<StampCard, StampCardReadDto>()
            .Map(dest => dest.Owners, src => src.GetOwners());
        config.NewConfig<Stamp, StampReadDto>()
            .TwoWays()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Reason, src => src.Reason);
    }
}