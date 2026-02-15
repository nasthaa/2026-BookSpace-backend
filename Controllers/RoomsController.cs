using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookSpace.Api.Data;
using BookSpace.Api.DTOs;
using BookSpace.Api.Models;

namespace BookSpace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly AppDbContext _context;

    public RoomsController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/rooms?page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetRooms(int page = 1, int pageSize = 10)
    {
        var query = _context.Rooms
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name);

        var total = await query.CountAsync();

        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RoomResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Capacity = r.Capacity,
                Location = r.Location
            })
            .ToListAsync();

        return Ok(new { total, data });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomResponseDto>> GetRoom(int id)
    {
        var room = await _context.Rooms
            .Where(r => r.Id == id && !r.IsDeleted)
            .Select(r => new RoomResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Capacity = r.Capacity,
                Location = r.Location
            })
            .FirstOrDefaultAsync();

        if (room == null) return NotFound();

        return Ok(room);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom(CreateRoomDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var exists = await _context.Rooms
            .AnyAsync(x => x.Name == dto.Name && !x.IsDeleted);

        if (exists)
            return BadRequest(new { message = "Nama ruangan sudah ada" });

        var room = new Room
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            Location = dto.Location,
            IsDeleted = false
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(int id, UpdateRoomDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var room = await _context.Rooms.FindAsync(id);
        if (room == null || room.IsDeleted) return NotFound();

        var exists = await _context.Rooms
            .AnyAsync(x => x.Name == dto.Name && x.Id != id && !x.IsDeleted);

        if (exists)
            return BadRequest(new { message = "The room name already exists." });

        room.Name = dto.Name;
        room.Capacity = dto.Capacity;
        room.Location = dto.Location;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null || room.IsDeleted) return NotFound();

        room.IsDeleted = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
