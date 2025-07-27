using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ServerServices.LectorSchedule.Entities.Base;

namespace ServerServices.LectorSchedule.Entities;

[Index(nameof(Name), IsUnique = true)]
[Index(nameof(ScheduleId), IsUnique = true)]
public class Room : Entity
{
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    public Guid ScheduleId { get; set; }
}