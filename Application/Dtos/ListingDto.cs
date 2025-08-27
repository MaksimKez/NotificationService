namespace Application.Dtos;

public class ListingDto
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public decimal AreaMeterSqr { get; set; }
    public int Rooms { get; set; }
    public int Floor { get; set; }
    public bool IsFurnished { get; set; }
    public bool PetsAllowed { get; set; }
    public bool HasBalcony { get; set; }
    public bool HasAppliances { get; set; }
    public string Url { get; set; }
    public DateTime CreatedAt { get; set; }
}