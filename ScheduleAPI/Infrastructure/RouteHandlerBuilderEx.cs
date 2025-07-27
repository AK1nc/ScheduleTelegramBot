namespace ScheduleAPI.Infrastructure;

internal static class RouteHandlerBuilderEx
{
    public static RouteHandlerBuilder Param(this RouteHandlerBuilder route, string Name, string Description)=> route.WithOpenApi(opt =>
    {
        if (opt.Parameters.FirstOrDefault(p => p.Name == Name) is not { } parameter)
            return opt;

        parameter.Description = Description;

        return opt;
    });
}
