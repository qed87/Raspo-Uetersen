namespace Raspo.StampCard.Web.Dtos;

public class StampReadDto
{
    public Guid Id { get; set; }

    public string Reason { get; set; } = null!;
}