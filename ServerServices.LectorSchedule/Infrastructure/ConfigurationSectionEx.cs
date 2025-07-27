using Microsoft.Extensions.Configuration;

namespace ServerServices.LectorSchedule.Infrastructure;

internal static class ConfigurationSectionEx
{
    public static void Deconstruct(this IConfigurationSection section, out string Name, out string? Value) =>
        (Name, Value) = (section.Key, section.Value);
}
