using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookSpace.Api.Data;
using BookSpace.Api.DTOs;
using BookSpace.Api.Models;

namespace BookSpace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public BookingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
    {
        return await _context.Bookings.Include(b => b.Room).ToListAsync();
    }

    [HttpPost]
    [HttpPost]
    public async Task<ActionResult> CreateBooking(CreateBookingDto dto)
    {
        bool bentrok = await _context.Bookings.AnyAsync(b =>
            b.RoomId == dto.RoomId &&
            dto.StartTime < b.EndTime &&
            dto.EndTime > b.StartTime
        );

        if (bentrok)
            return BadRequest("Ruangan sudah dibooking di waktu tersebut.");

        var booking = new Booking
        {
            BorrowerName = dto.BorrowerName,
            RoomId = dto.RoomId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = "Pending"
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBookings), new { id = booking.Id }, booking);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateBooking(int id, UpdateBookingDto dto)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
            return NotFound("Booking tidak ditemukan");

        bool bentrok = await _context.Bookings.AnyAsync(b =>
            b.Id != id &&
            b.RoomId == dto.RoomId &&
            dto.StartTime < b.EndTime &&
            dto.EndTime > b.StartTime
        );

        if (bentrok)
            return BadRequest("Ruangan sudah dibooking di waktu tersebut.");

        booking.RoomId = dto.RoomId;
        booking.StartTime = dto.StartTime;
        booking.EndTime = dto.EndTime;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateBookingStatus(int id, UpdateBookingStatusDto dto)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
            return NotFound("Booking tidak ditemukan");

        var allowed = new[] { "Pending", "Approved", "Rejected" };

        if (!allowed.Contains(dto.Status))
            return BadRequest("Status tidak valid");

        booking.Status = dto.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
