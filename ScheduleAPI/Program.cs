using System.Globalization;

using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using ScheduleAPI.EndPoints;
using ScheduleAPI.Infrastructure.Middleware;

using ServerServices.LectorSchedule;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.Configure<JsonOptions>(opt => opt.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

//builder.Logging.AddSysLog("192.168.1.68");

builder.Services.Configure<KestrelServerOptions>(o => o.AllowSynchronousIO = true);

builder.Services
    .AddScheduleServices()
    .AddScheduleServiceDB(builder.Configuration.GetSection("db:Schedule"));

var app = builder.Build();

app.MapHealthChecks("/health");

app.UseMiddleware<PrintRequestMiddleware>();

//if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger().UseSwaggerUI();
}

app.UseMiddleware<ResponseTimeHeaderMiddleware>();

app.MapScheduleApi();
app.MapIndexApi();

//if (app.Environment.IsDevelopment())
{
    app.MapSystemApi();
}

app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();

app.Run();

