namespace BookSpace.Api.DTOs;

public class CreateBookingDto
{
    public string BorrowerName { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class UpdateBookingDto
{
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class UpdateBookingStatusDto
{
    public string Status { get; set; } = string.Empty; // Pending | Approved | Rejected
}
