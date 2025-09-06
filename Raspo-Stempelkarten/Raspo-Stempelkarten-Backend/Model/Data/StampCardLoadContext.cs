namespace Raspo_Stempelkarten_Backend.Model.Data;

public class StampCardLoadContext
{
    public List<StampCardData> StampCards { get; set; } = [];

    public List<StampData> Stamps { get; set; } = [];
}