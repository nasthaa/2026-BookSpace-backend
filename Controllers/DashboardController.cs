using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookSpace.Api.Data;

namespace BookSpace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardStats()
    {
        var today = DateTime.Today;

        var totalRooms = await _context.Rooms.CountAsync();

        var pendingRequests = await _context.Bookings
            .CountAsync(book => book.Status == "Pending" && !book.IsDeleted);

        var bookingsToday = await _context.Bookings
            .CountAsync(book =>
                !book.IsDeleted &&
                book.StartTime.Date == today
            );

        return Ok(new
        {
            totalRooms,
            bookingsToday,
            pendingRequests
        });
    }
}
