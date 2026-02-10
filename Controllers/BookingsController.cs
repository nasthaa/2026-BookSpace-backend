using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookSpace.Api.Data;
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
    public async Task<ActionResult> CreateBooking(Booking booking)
    {
        bool bentrok = await _context.Bookings.AnyAsync(b =>
            b.RoomId == booking.RoomId &&
            booking.StartTime < b.EndTime &&
            booking.EndTime > b.StartTime
        );

        if (bentrok)
        {
            return BadRequest("Ruangan sudah dibooking di waktu tersebut.");
        }

        booking.Status = "Approved";

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return Ok(booking);
    }
}
