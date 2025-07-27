using System.Text.Json.Serialization;

namespace ServerServices.LectorSchedule.Infrastructure;

internal class JsonConverterAttribute<TConverter>() : JsonConverterAttribute(typeof(TConverter)) where TConverter : JsonConverter;
