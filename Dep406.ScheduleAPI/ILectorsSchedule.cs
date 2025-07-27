using Refit;

namespace ScheduleTelegramBot.ScheduleAPI;

public interface ILectorSchedule
{
    [Get("/schedule/lector/id/{Id}")]
    Task<ScheduleLector> GetById(Guid Id, CancellationToken Cancel = default);

    [Get("/schedule/lector/name/{Name}")]
    Task<ScheduleLector> GetByName(string Name, CancellationToken Cancel = default);

    [Get("/index/lectors/name/similar")]
    Task<LectorInfo[]> GetLectorBySimilarName(string Name, int? Skip = null, int? Take = null, int? Page = null, CancellationToken Cancel = default);
}

