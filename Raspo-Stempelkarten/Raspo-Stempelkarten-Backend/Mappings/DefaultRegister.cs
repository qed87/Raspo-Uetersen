using Mapster;
using QRCoder;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Mappings;

public class DefaultRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // var generator = new PayloadGenerator.Url($"https://localhost:5047/api/teams/{team}/seasons/{season}/stampcard/{stampCardId}/stamp/{id}");
        // var payload = generator.ToString();
        //
        // var qrGenerator = new QRCodeGenerator();
        // var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        // var qrCodeRawData = qrCodeData.GetRawData(QRCodeData.Compression.GZip);
        // PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        // var qrCodeAsBitmap = qrCode.GetGraphic(20);
        
        config.NewConfig<StampCard, StampCardReadDetailsDto>()
            .Map(dest => dest.Stamps, src => src.GetStamps())
            .Map(dest => dest.ActualStamps, src => src.GetStamps().Count());
        config.NewConfig<StampCard, StampCardReadDto>();
        config.NewConfig<Stamp, StampReadDto>()
            .TwoWays()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Reason, src => src.Reason);
    }
}