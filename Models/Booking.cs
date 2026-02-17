namespace BookSpace.Api.Models;

public class Booking
{
    public int Id { get; set; }
    public string BorrowerName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "Pending";
    public int RoomId { get; set; }
    public Room? Room { get; set; }
    public bool IsDeleted { get; set; } = false;
}