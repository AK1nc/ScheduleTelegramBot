using Microsoft.AspNetCore.Http.HttpResults;

namespace ScheduleAPI.Infrastructure;

internal static class TypedResultsEx
{
    public static Ok<T> Ok<T>(this T? value) => TypedResults.Ok(value);

    public static BadRequest<T> BadRequest<T>(this T? value) => TypedResults.BadRequest(value);

    public static NotFound<T> NotFound<T>(this T? value) => TypedResults.NotFound(value);
}
