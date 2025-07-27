namespace ServerServices.LectorSchedule;

public readonly record struct FreeRooms
{
    public IReadOnlyCollection<string> Free { get; init; }

    public IDictionary<string, IReadOnlyCollection<ScheduleStudentLesson>> Used { get; init; }
}
