//using System.Text.Json;
//using Microsoft.AspNetCore.Mvc.ModelBinding;

//namespace ScheduleAPI.Infrastructure.Binders;

//internal class DateTimeBinder : IModelBinder
//{
//    public async Task BindModelAsync(ModelBindingContext Context)
//    {
//        var json = await new StreamReader(Context.HttpContext.Request.Body).ReadToEndAsync();
//        var customer = JsonSerializer.Deserialize<DateTime>(json);
//        Context.Result = ModelBindingResult.Success(customer);
//    }
//}
