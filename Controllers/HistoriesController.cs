using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookSpace.Api.Data;
using BookSpace.Api.Models;

namespace BookSpace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public HistoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetHistories( int page = 1,  int pageSize = 8, string? search = null, string? status = null)
    {
        var now = DateTime.Now;

        var raw = await _context.Bookings
            .Include(b => b.Room)
            .ToListAsync();

        var data = raw
            .Select(book => new
            {
                Booking = book,
                Status = GetActualStatus(book, now)
            });

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            data = data.Where(found =>
                found.Booking.BorrowerName.ToLower().Contains(search) ||
                found.Booking.Room!.Name.ToLower().Contains(search)
            );
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            data = data.Where(x => x.Status == status);
        }

        var ordered = data
            .OrderBy(x => x.Status switch
            {
                "Pending" => 1,
                "OnGoing" => 2,
                "Approved" => 3,
                "Completed" => 4,
                "Expired" => 5,
                "Rejected" => 6,
                "Deleted" => 7,
                _ => 99
            })
            .ThenBy(x => x.Booking.StartTime);

        var total = ordered.Count();

        var paged = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Booking.Id,
                x.Booking.BorrowerName,
                x.Booking.StartTime,
                x.Booking.EndTime,
                Status = x.Status,
                x.Booking.RoomId,
                Room = new
                {
                    x.Booking.Room!.Id,
                    x.Booking.Room.Name
                }
            });

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
