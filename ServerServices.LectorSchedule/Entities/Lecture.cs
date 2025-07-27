using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

using ServerServices.LectorSchedule.Entities.Base;

namespace ServerServices.LectorSchedule.Entities;

[Index(nameof(Name), IsUnique = false)]
[Index(nameof(ScheduleId), IsUnique = true)]
public class Lector : Entity
{
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    public Guid ScheduleId { get; set; }
}