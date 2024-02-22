using Serilog.Configuration;
using Serilog.Events;

namespace Serilog.Sinks.DiscordExtended
{
    public static class DiscordSinkExtensions
    {
        public static LoggerConfiguration Discord(this LoggerSinkConfiguration loggerConfiguration,
                                                  ulong webhookId,
                                                  string webhookToken,
                                                  IFormatProvider? formatProvider = null,
                                                  LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
                                                  bool includeProperties = false) =>
            loggerConfiguration.Sink(new DiscordSink(formatProvider,
                                                     webhookId,
                                                     webhookToken,
                                                     restrictedToMinimumLevel,
                                                     includeProperties));
    }
}
