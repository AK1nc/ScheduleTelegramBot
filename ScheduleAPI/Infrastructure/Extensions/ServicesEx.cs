namespace ScheduleAPI.Infrastructure.Extensions;

internal static class ServicesEx
{
    public static T Required<T>(this IServiceScope scope) where T : notnull => scope.ServiceProvider.Required<T>();

    public static T Required<T>(this IServiceProvider services) where T : notnull => services.GetRequiredService<T>();
}
