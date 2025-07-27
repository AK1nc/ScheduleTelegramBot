using Microsoft.EntityFrameworkCore;

using ServerServices.LectorSchedule.Entities;

namespace ServerServices.LectorSchedule;

public class LectorScheduleDB(DbContextOptions opts) : DbContext(opts)
{
    public virtual DbSet<Lector> Lectors { get; set; }

    public virtual DbSet<StudentsGroup> Groups { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }
}
