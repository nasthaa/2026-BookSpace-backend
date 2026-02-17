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
    public async Task<ActionResult> GetBookings(int page = 1, int pageSize = 8)
    {
        var now = DateTime.Now;

        var raw = await _context.Bookings
            .Include(book => book.Room)
            .Where(book => !book.IsDeleted)
            .ToListAsync();

        var data = raw
            .Select(book => new
            {
                Booking = book,
                Status = GetActualStatus(book, now)
            })
            .Where(book =>
                book.Status == "Pending" ||
                book.Status == "Approved" ||
                book.Status == "OnGoing"
            )
            .OrderBy(book => book.Status switch
            {
                "Pending" => 1,
                "OnGoing" => 2,
                "Approved" => 3,
                _ => 4
            })
            .ThenBy(book => book.Booking.StartTime)
            .ThenBy(book => book.Booking.Room!.Name)
            .Select(book => new
            {
                book.Booking.Id,
                book.Booking.BorrowerName,
                book.Booking.StartTime,
                book.Booking.EndTime,
                Status = book.Status,
                book.Booking.RoomId,
                Room = new
                {
                    book.Booking.Room!.Id,
                    book.Booking.Room.Name
                }
            });

        var total = data.Count();

        var paged = data
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return Ok(new { total, data = paged });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetBooking(int id)
    {
        var now = DateTime.Now;

        var book = await _context.Bookings
            .Include(book => book.Room)
            .FirstOrDefaultAsync(book => book.Id == id);

        if (book == null) return NotFound();

        return Ok(new
        {
            book.Id,
            book.BorrowerName,
            book.StartTime,
            book.EndTime,
            Status = GetActualStatus(book, now),
            book.RoomId,
            Room = new { book.Room!.Id, book.Room.Name }
        });
    }

    [HttpPost]
    public async Task<ActionResult> CreateBooking(CreateBookingDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var now = DateTime.Now;

        if (dto.StartTime < now)
            return BadRequest(new { message = "Start time cannot be less than today." });

        if (dto.EndTime <= dto.StartTime)
            return BadRequest(new { message = "End must be after start." });

        bool crash = await _context.Bookings.AnyAsync(book =>
            book.RoomId == dto.RoomId &&
            dto.StartTime < book.EndTime &&
            dto.EndTime > book.StartTime &&
            !book.IsDeleted
        );

        if (crash)
            return BadRequest(new { message = "Room already booked." });

        var booking = new Booking
        {
            BorrowerName = dto.BorrowerName,
            RoomId = dto.RoomId!.Value,
            StartTime = dto.StartTime!.Value,
            EndTime = dto.EndTime!.Value,
            Status = "Pending"
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateBooking(int id, UpdateBookingDto dto)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        var now = DateTime.Now;
        var actualStatus = GetActualStatus(booking, now);

        if (actualStatus is "Rejected" or "Completed" or "Expired" or "Deleted")
            return BadRequest(new { message = "Cannot edit this booking." });
        
        if (dto.StartTime < now)
            return BadRequest(new { message = "Start time cannot be less than today." });

        if (dto.EndTime <= dto.StartTime)
            return BadRequest(new { message = "End must be after start." });

        bool crash = await _context.Bookings.AnyAsync(book =>
            book.Id != id &&
            book.RoomId == dto.RoomId &&
            dto.StartTime < book.EndTime &&
            dto.EndTime > book.StartTime &&
            !book.IsDeleted
        );

        if (crash)
            return BadRequest(new { message = "Room already booked." });

        booking.RoomId = dto.RoomId!.Value;
        booking.StartTime = dto.StartTime!.Value;
        booking.EndTime = dto.EndTime!.Value;
        booking.Status = "Pending";

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateBookingStatus(int id, UpdateBookingStatusDto dto)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        var allowed = new[] { "Approved", "Rejected", "Cancelled" };

        if (!allowed.Contains(dto.Status))
            return BadRequest(new { message = "Invalid status" });

        booking.Status = dto.Status;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteBooking(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        booking.IsDeleted = true;
        booking.Status = "Deleted";
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static string GetActualStatus(Booking book, DateTime now)
    {
        if (book.IsDeleted) return "Deleted";
        // if (book.Status == "Cancelled") return "Cancelled";
        if (book.Status == "Rejected") return "Rejected";

        if (now < book.StartTime)
            return book.Status;

        if (now >= book.StartTime && now <= book.EndTime)
        {
            if (book.Status == "Approved") return "OnGoing";
            if (book.Status == "Pending") return "Expired";
        }

        if (now > book.EndTime)
        {
            if (book.Status == "Approved") return "Completed";
            if (book.Status == "Pending") return "Expired";
        }

        return book.Status;
    }
}
