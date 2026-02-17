using System.ComponentModel.DataAnnotations;

namespace BookSpace.Api.DTOs;

public class CreateBookingDto
{
    [Required(ErrorMessage = "Borrower name is required.")]
    public string BorrowerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Room must be selected.")]
    public int? RoomId { get; set; }

    [Required(ErrorMessage = "Start time is required.")]
    public DateTime? StartTime { get; set; }

    [Required(ErrorMessage = "End time is required.")]
    public DateTime? EndTime { get; set; }
}

public class UpdateBookingDto
{
    [Required(ErrorMessage = "Room must be selected.")]
    public int? RoomId { get; set; }

    [Required(ErrorMessage = "Start time is required.")]
    public DateTime? StartTime { get; set; }

    [Required(ErrorMessage = "End time is required.")]
    public DateTime? EndTime { get; set; }
}

public class UpdateBookingStatusDto
{
    [Required]
    [RegularExpression("Approved|Rejected")]
    public string Status { get; set; } = string.Empty;
}