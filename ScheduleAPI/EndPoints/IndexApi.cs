using System.ComponentModel;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Patterns;

using ScheduleAPI.Infrastructure;

using ServerServices.LectorSchedule;

namespace ScheduleAPI.EndPoints;

internal static class IndexApi
{
    public static IEndpointRouteBuilder MapIndexApi(this IEndpointRouteBuilder route, string url = "index", params string[] tags)
    {
        route.MapGroup(url)
             .MapIndexApi()
             .WithTags(tags is [] ? ["Index"] : tags)
             .WithOpenApi();

        return route;
    }

    public static RouteGroupBuilder MapIndexApi(this RouteGroupBuilder index)
    {
        index.MapDelete("", ClearIndex);
        index.MapDelete("cache", ClearCache);

        index.MapPost("build/lector/{StartId:guid}", BuildIndexWithLectorId);
        index.MapPost("build/students/{GroupName?}", BuildIndexWithGroupName);

        index.MapGet("name/similar", GetNameSimilar).SkipTakePageParams();

        var lectors = index.MapGroup("lectors");
        lectors.MapGet("", GetAllLectors).SkipTakePageParams();
        lectors.MapGet("id", GetAllLectorsIds).SkipTakePageParams();
        lectors.MapGet("name", GetAllLectorsNames).SkipTakePageParams();
        lectors.MapGet("name/similar", GetLectorSimilarName);
        lectors.MapGet("count", GetLectorsCount);
        lectors.MapGet("count/pages/{size:int}", GetLectorsCountPages).Param("size", "Размер страницы");


        var students = index.MapGroup("students");
        students.MapGet("", GetAllStudents).SkipTakePageParams();
        students.MapGet("id", GetAllStudentsIds).SkipTakePageParams();
        students.MapGet("names", GetAllStudentsNames).SkipTakePageParams();
        students.MapGet("names/similar", GetStudentsSimilarName).SkipTakePageParams();
        students.MapGet("count", GetStudentsCount);
        students.MapGet("count/pages/{size:int}", GetStudentsCountPages).Param("size", "Размер страницы");

        return index;
    }

    private static void SkipTakePageParams(this RouteHandlerBuilder route) => route
        .Param("Skip", "Пропустить указанное количество записей")
        .Param("Take", "Взять указанное количество записей")
        .Param("Page", "Выбрать указанную страницу начиная с номера 0");

    #region Clear

    [EndpointName("ClearIndex")]
    [EndpointSummary("Очистить индекс")]
    [EndpointDescription("Удалить базу данных индекса")]
    //[ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    private static async Task<Ok<bool>> ClearIndex(ILectorsStore store, CancellationToken Cancel)
    {
        return TypedResults.Ok(await store.ClearAsync(Cancel).ConfigureAwait(false));
    }

    [EndpointName("ClearCache")]
    [EndpointSummary("Очистить кеш")]
    [EndpointDescription("Удалить файлы кеша")]
    //[ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    private static async Task<Ok<bool>> ClearCache(IScheduleCache cache, CancellationToken Cancel)
    {
        return TypedResults.Ok(await cache.ClearAsync(Cancel));
    }

    #endregion

    #region Build

    [EndpointSummary("Индекс на основе лектора")]
    [EndpointDescription("Сформировать индекс путём обхода графа расписания от указанного первого лектора")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    private static async Task<Ok<int>> BuildIndexWithLectorId(
        [DefaultValue("0fb94e9b-1d9b-11e0-9baf-1c6f65450efa")] Guid? StartId,
        [DefaultValue(false)] bool? ClearIndex,
        [DefaultValue(false)] bool? ClearCache,
        ILectorsIndex index,
        CancellationToken Cancel)
    {
        var count = await index.BuildByLectorIdAsync(
            StartId ?? Guid.Parse("0fb94e9b-1d9b-11e0-9baf-1c6f65450efa"),
            ClearIndex ?? false,
            ClearCache ?? false,
            Cancel);

        return TypedResults.Ok(count);
    }

    [EndpointSummary("Индекс на основе студенческой группы")]
    [EndpointDescription("Сформировать индекс путём обхода графа расписания от указанной первой группы студентов")]
    //[ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    private static async Task<Ok<int>> BuildIndexWithGroupName(
        [DefaultValue("М4О-506С-20")] string? GroupName,
        [DefaultValue(false)] bool? ClearIndex,
        [DefaultValue(false)] bool? ClearCache,
        ILectorsIndex index,
        CancellationToken Cancel)
    {
        var count = await index.BuildByGroupNameAsync(
            GroupName ?? "М4О-506С-20",
            ClearIndex ?? false,
            ClearCache ?? false,
            Cancel);

        return TypedResults.Ok(count);
    }

    #endregion

    #region Lectors

    [EndpointSummary("Список лекторов")]
    [EndpointDescription("Получить список лекторов с указанием их имени и идентификатора, содержащихся в индексе")]
    //[ProducesResponseType(typeof(IEnumerable<ILectorsStore.LectorIdInfo>), StatusCodes.Status200OK)]
    private static async Task<Ok<IEnumerable<ILectorsStore.LectorIdInfo>>> GetAllLectors(
        int? Skip,
        int? Take,
        int? Page,
        ILectorsStore store,
        CancellationToken Cancel)
    {
        if (Page is > 0 and var page && Take is > 0 and var take)
            Skip = page * take;

        var items = await store.GetLectorsAsync(Skip ?? 0, Take ?? 0, Cancel);
        return TypedResults.Ok(items);
    }

    [EndpointSummary("Идентификаторы лекторов")]
    [EndpointDescription("Получить список идентификаторов лекторов, содержащихся в индексе")]
    //[ProducesResponseType(typeof(IEnumerable<Guid>), StatusCodes.Status200OK)]
    private static async Task<Ok<IEnumerable<Guid>>> GetAllLectorsIds(
        int? Skip,
        int? Take,
        int? Page,
        ILectorsStore store,
        CancellationToken Cancel)
    {
        if (Page is > 0 and var page && Take is > 0 and var take)
            Skip = page * take;

        var items = await store.GetLectorsAsync(Skip ?? 0, Take ?? 0, Cancel);
        return TypedResults.Ok(items.Select(l => l.ScheduleId));
    }

    [EndpointSummary("Имена лекторов")]
    [EndpointDescription("Получить список имён лекторов, содержащихся в индексе")]
    //[ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    private static async Task<Ok<IEnumerable<string>>> GetAllLectorsNames(
        int? Skip,
        int? Take,
        int? Page,
        ILectorsStore store,
        CancellationToken Cancel)
    {
        if (Page is > 0 and var page && Take is > 0 and var take)
            Skip = page * take;

        var items = await store.GetLectorsAsync(Skip ?? 0, Take ?? 0, Cancel);
        return TypedResults.Ok(items.Select(l => l.Name));
    }

    [EndpointSummary("Имена лекторов")]
    [EndpointDescription("Получить список имён лекторов, содержащихся в индексе")]
    //[ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    private static async Task<Ok<IEnumerable<ILectorsStore.LectorIdInfo>>> GetLectorSimilarName(
        string Name,
        int? Skip,
        int? Take,
        int? Page,
        ILectorsStore store,
        CancellationToken Cancel)
    {
        if (Page is > 0 and var page && Take is > 0 and var take)
            Skip = page * take;

        var items = await store.GetLectorsSimilarNameAsync(Name, Skip ?? 0, Take ?? 0, Cancel);
        return items.Ok();
    }

    [EndpointSummary("Количество лекторов")]
    [EndpointDescription("Получить количество лекторов в индексе")]
    //[ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    private static async Task<Ok<int>> GetLectorsCount(ILectorsStore store, CancellationToken Cancel)
    {
        var count = await store.GetLectorsCountAsync(Cancel);
        return TypedResults.Ok(count);
    }

    [EndpointSummary("Число страниц списка лекторов")]
    [EndpointDescription("Рассчитать количество страниц указанного размера списка лекторов")]
    //[ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    private static async Task<Results<Ok<int>, BadRequest<string>>> GetLectorsCountPages(
        int Size, 
        ILectorsStore store, 
        CancellationToken Cancel)
    {
        if (Size <= 0) return TypedResults.BadRequest("Размер страницы должен быть положительным числом");

        var count = await store.GetLectorsCountAsync(Cancel);
        var pages_count = (int)Math.Ceiling((double)count / Size);
        return TypedResults.Ok(pages_count);
    }

    #endregion

    #region Students

    [EndpointSummary("Список групп студентов")]
    [EndpointDescription("Получить список групп студентов, содержащихся в индексе")]
    //[ProducesResponseType(typeof(IEnumerable<ILectorsStore.GroupIdInfo>), StatusCodes.Status200OK)]
    private static async Task<Ok<IEnumerable<ILectorsStore.GroupIdInfo>>> GetAllStudents(
        int? Skip,
        int? Take,
        int? Page,
        ILectorsStore store,
        CancellationToken Cancel)
    {
        if (Page is > 0 and var page && Take is > 0 and var take)
            Skip = page * take;

        var items = await store.GetGroupsAsync(Skip ?? 0, Take ?? 0, Cancel);
        return TypedResults.Ok(items);
    }

    [EndpointSummary("Список идентификаторов групп студентов")]
    [EndpointDescription("Получить список идентификаторов групп студентов, содержащихся в индексе")]
    //[ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    private static async Task<Ok<IEnumerable<string>>> GetAllStudentsIds(
        int? Skip,
        int? Take,
        int? Page,
        ILectorsStore store,
        CancellationToken Cancel)
    {
        if (Page is > 0 and var page && Take is > 0 and var take)
            Skip = page * take;

        var items = await store.GetGroupsAsync(Skip ?? 0, Take ?? 0, Cancel);
        return TypedResults.Ok(items.Select(l => l.ScheduleId));
    }

    [EndpointSummary("Список названий групп студентов")]
    [EndpointDescription("Получить список названий групп студентов, содержащихся в индексе")]
    //[ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    private static async Task<Ok<IEnumerable<string>>> GetAllStudentsNames(
        int? Skip,
        int? Take,
        int? Page,
        ILectorsStore store,
        CancellationToken Cancel)
    {
        if (Page is > 0 and var page && Take is > 0 and var take)
            Skip = page * take;

        var items = await store.GetGroupsAsync(Skip ?? 0, Take ?? 0, Cancel);
        return TypedResults.Ok(items.Select(l => l.Name));
    }

    private static async Task<Ok<IEnumerable<ILectorsStore.GroupIdInfo>>> GetStudentsSimilarName(
        string Name,
        int? Skip,
        int? Take,
        int? Page,
        ILectorsStore store,
        CancellationToken Cancel)
    {
        if (Page is > 0 and var page && Take is > 0 and var take)
            Skip = page * take;

        var items = await store.GetGroupsSimilarNameAsync(Name, Skip ?? 0, Take ?? 0, Cancel);
        return items.Ok();
    }

    [EndpointSummary("Количество групп студентов в индексе")]
    [EndpointDescription("Получить количество групп студентов в индексе")]
    //[ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    private static async Task<Ok<int>> GetStudentsCount(ILectorsStore store, CancellationToken Cancel)
    {
        var count = await store.GetGroupsCountAsync(Cancel);
        return TypedResults.Ok(count);
    }

    [EndpointSummary("Число страниц групп студентов")]
    [EndpointDescription("Рассчитать количество страниц указанного размера списка групп студентов в индексе")]
    //[ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    //[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    private static async Task<Results<Ok<int>, BadRequest<string>>> GetStudentsCountPages(
        int Size, 
        ILectorsStore store,
        CancellationToken Cancel)
    {
        if (Size <= 0) return TypedResults.BadRequest("Размер страницы должен быть положительным числом");

        var count = await store.GetGroupsCountAsync(Cancel);
        var pages_count = (int)Math.Ceiling((double)count / Size);
        return TypedResults.Ok(pages_count);
    }

    #endregion

    private static async Task<Results<Ok<IEnumerable<ILectorsStore.LectorIdInfo>>, Ok<IEnumerable<ILectorsStore.GroupIdInfo>>>> GetNameSimilar(
        string Name, 
        int? Skip, 
        int? Take, 
        int? Page,
        ILectorsStore store,
        CancellationToken Cancel)
    {
        if (Page is > 0 and var page && Take is > 0 and var take)
            Skip = page * take;

        if (Name.All(char.IsLetter))
            return await GetLectorSimilarName(Name, Skip, Take, Page, store, Cancel);

        return await GetStudentsSimilarName(Name, Skip, Take, Page, store, Cancel);
    }
}
