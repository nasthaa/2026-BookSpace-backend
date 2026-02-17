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

    [HttpGet]
    public async Task<ActionResult> GetRooms(int page = 1, int pageSize = 8)
    {
        var query = _context.Rooms
            .Where(index => !index.IsDeleted)
            .OrderBy(index => index.Name);

        var total = await query.CountAsync();

        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(room => new RoomResponseDto
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity,
                Location = room.Location
            })
            .ToListAsync();

        return Ok(new { total, data });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomResponseDto>> GetRoom(int id)
    {
        var room = await _context.Rooms
            .Where(room => room.Id == id && !room.IsDeleted)
            .Select(room => new RoomResponseDto
            {
                Id = room.Id,
                Name = room.Name,
                Capacity = room.Capacity,
                Location = room.Location
            })
            .FirstOrDefaultAsync();

        if (room == null) return NotFound();

        return Ok(room);
    }

    [HttpPost]
    public async Task<ActionResult> CreateRoom(CreateRoomDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var exists = await _context.Rooms
            .AnyAsync(index => index.Name == dto.Name && !index.IsDeleted);

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
    public async Task<ActionResult> UpdateRoom(int id, UpdateRoomDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var room = await _context.Rooms.FindAsync(id);
        if (room == null || room.IsDeleted) return NotFound();

        var exists = await _context.Rooms
            .AnyAsync(index => index.Name == dto.Name && index.Id != id && !index.IsDeleted);

        if (exists)
            return BadRequest(new { message = "The room name already exists." });

        room.Name = dto.Name;
        room.Capacity = dto.Capacity;
        room.Location = dto.Location;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null || room.IsDeleted) return NotFound();

        room.IsDeleted = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
