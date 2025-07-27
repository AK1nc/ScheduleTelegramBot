using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ServerServices.LectorSchedule.Entities.Base;

namespace ServerServices.LectorSchedule.Entities;

[Index(nameof(Name), IsUnique = true)]
[Index(nameof(ScheduleId), IsUnique = true)]
public class StudentsGroup : Entity
{
    [MaxLength(30)] public string Name { get; set; } = null!;

    [MaxLength(32)] public string ScheduleId { get; set; } = null!;
}