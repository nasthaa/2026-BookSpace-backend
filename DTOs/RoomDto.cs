using System.ComponentModel.DataAnnotations;

namespace BookSpace.Api.DTOs;

public class CreateRoomDto 
{
    [Required(ErrorMessage = "Room name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Room capacity is required.")]
    [Range(30, 300, ErrorMessage = "Capacity must be between 30 and 300.")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "Room location is required.")]
    public string Location { get; set; } = string.Empty;
}
public class UpdateRoomDto
{
    [Required(ErrorMessage = "Room name is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Room capacity is required.")]
    [Range(30, 300, ErrorMessage = "Capacity must be between 30 and 300.")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "Room location is required.")]
    public string Location { get; set; } = string.Empty;
}

public class RoomResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Location { get; set; } = string.Empty;
}
