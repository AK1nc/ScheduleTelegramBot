using ScheduleAPI.EndPoints.Schedule;

namespace ScheduleAPI.EndPoints;

internal static class ScheduleApi
{
    public static IEndpointRouteBuilder MapScheduleApi(this IEndpointRouteBuilder app, string url = "schedule")
    {
        app.MapGroup(url)
           .MapScheduleApi()
           //.WithTags(tags is [] ? ["Schedule"] : tags)
           .WithOpenApi()
           ;

        return app;
    }

    public static RouteGroupBuilder MapScheduleApi(this RouteGroupBuilder schedule_group)
    {
        schedule_group
            .MapScheduleLectorsApi()
            .MapScheduleStudentsApi()
            .MapScheduleRoomsApi()
            ;
        return schedule_group;
    }
}
