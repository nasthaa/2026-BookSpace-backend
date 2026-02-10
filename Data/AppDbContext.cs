using Microsoft.EntityFrameworkCore;
using BookSpace.Api.Models;

namespace BookSpace.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Room> Rooms { get; set; }
    public DbSet<Booking> Bookings { get; set; }
}
