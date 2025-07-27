using System.Text.Json.Serialization;
using MathCore;

namespace ServerServices.LectorSchedule;

public record ScheduleRoom(
    string Name,
    Guid Id,
    DateTime Created,
    IReadOnlyList<ScheduleRoomLesson> Lessons)
{
    [JsonIgnore]
    public IEnumerable<ScheduleRoomLesson> LessonsCombined
    {
        get
        {
            // Здесь будем хранить предыдущий просмотренный урок
            ScheduleRoomLesson? first_lesson = null;

            // Просматриваем все занятия
            foreach (var lesson in Lessons) 
            {
                // Если предыдущий просмотренный урок был
                // и
                // если разница во времени между концом предыдущего и началом текущего занятия менее 45 минут
                if (first_lesson is not null
                    && lesson.Name == first_lesson.Name
                    && (lesson.Start - (first_lesson.Start + first_lesson.Duration)).TotalMinutes <= 45)
                {
                    // Формируем объединённый урок сливая предыдущий и текущий

                    var lesson_end = (lesson.Start + lesson.Duration) - first_lesson.Start;
                    var lessons_types = first_lesson.LessonType.Concat(lesson.LessonType).Distinct().ToArray();
                    var lectors = first_lesson.Lectors.Combine(lesson.Lectors);
                    var lessons_groups = first_lesson.Groups.Concat(lesson.Groups).Distinct().ToArray();

                    first_lesson = new(
                        lesson.Name,
                        first_lesson.Start,
                        lesson_end,
                        lectors,
                        lessons_types,
                        lessons_groups
                    );

                    // сформированный урок назначаем предыдущим и переходим к следующему:
                    // а вдруг следующий тоже можно объединить?
                    continue;
                }

                // Если был предыдущий урок, то возвращаем именно его, а не текущий
                if (first_lesson is not null)
                    yield return first_lesson;

                // , а текущий назначаем предыдущим
                first_lesson = lesson;
            }

            // Если после просмотра всех уроков предыдущий был установлен, то его нужно вернуть как результат.
            if (first_lesson is not null)
                yield return first_lesson;
        }
    }
}

public record ScheduleRoomLesson(
    string Name,
    DateTime Start,
    TimeSpan Duration,
    IReadOnlyDictionary<Guid, string> Lectors,
    IReadOnlyList<string> LessonType,
    IReadOnlyList<string> Groups)
{
    [JsonIgnore]
    public DateTime End => Start + Duration;

    [JsonIgnore]
    public Interval<DateTime> Interval => new(Start, End, true);
}