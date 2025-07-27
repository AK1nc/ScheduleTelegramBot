using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ScheduleAPI.Infrastructure.Logging;

internal static class SysLogLoggerEx
{
    public static ILoggingBuilder AddSysLog(this ILoggingBuilder log, string Host = "localhost", int Port = 514, Func<string, LogLevel, bool>? filter = null) => log.
        AddProvider(new SysLogLoggerProvider(Host, Port, filter));

    public static ILoggerFactory AddSysLog(this ILoggerFactory log, string Host, int Port, Func<string, LogLevel, bool>? filter = null)
    {
        log.AddProvider(new SysLogLoggerProvider(Host, Port, filter));
        return log;
    }
}

internal class SysLogLoggerProvider(string Host, int Port, Func<string, LogLevel, bool>? filter) : ILoggerProvider
{
    private readonly UdpClient _Udp = GetUDP(Host, Port);

    private static UdpClient GetUDP(string Host, int Port)
    {
        var udp = new UdpClient();
        udp.Connect(Host, Port);
        return udp;
    }

    public ILogger CreateLogger(string category) => new SysLogLogger(category, Host, _Udp, filter);

    public void Dispose() => _Udp.Dispose();
}

internal class SysLogLogger(string Category, string Host, UdpClient udp, Func<string, LogLevel, bool>? filter) : ILogger
{

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel Level) => filter?.Invoke(Category, Level) ?? true;

    public void Log<TState>(LogLevel Level, EventId EventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if(!IsEnabled(Level)) return;
        formatter.NotNull();

        if(formatter(state, exception) is not { Length: > 0 } msg)
            return;

        var message = new StringBuilder()
            .Append(Level).Append(':').Append(' ').Append(msg);

        if (exception is not null)
            message.AppendLine().AppendLine().Append(exception);

        const int sys_log_facility = 16;
        var syslog_level = sys_log_facility * 8 + (int)MapToSyslogLevel(Level);

        var log_msg_bytes = Encoding.UTF8.GetBytes($"<{syslog_level}>{Host} {message}");
        udp.Send(log_msg_bytes);
        return;

        static SyslogLogLevel MapToSyslogLevel(LogLevel level) => level switch
        {
            LogLevel.Critical => SyslogLogLevel.critical,
            LogLevel.Error => SyslogLogLevel.error,
            LogLevel.Warning => SyslogLogLevel.warn,
            LogLevel.Information => SyslogLogLevel.info,
            LogLevel.Trace => SyslogLogLevel.info,
            LogLevel.Debug => SyslogLogLevel.debug,
            _ => SyslogLogLevel.info
        };
    }
}

public enum SyslogLogLevel
{
    emergency,
    alert,
    critical,
    error,
    warn,
    notice,
    info,
    debug
}