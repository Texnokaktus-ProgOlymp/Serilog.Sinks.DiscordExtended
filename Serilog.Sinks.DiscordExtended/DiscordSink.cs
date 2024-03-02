using Discord;
using Discord.Webhook;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.DiscordExtended
{
    public class DiscordSink(IFormatProvider? formatProvider,
                             ulong webhookId,
                             string webhookToken,
                             LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information,
                             bool includeProperties = false) : ILogEventSink
    {
        private const int MaxFieldLength = 1000;

        public void Emit(LogEvent logEvent)
        {
            if (!ShouldLogMessage(restrictedToMinimumLevel, logEvent.Level))
                return;

            var embedBuilder = new EmbedBuilder();
            using var webHook = new DiscordWebhookClient(webhookId, webhookToken);

            try
            {
                if (logEvent.Exception is { } exception)
                    AddExceptionToEmbedBuilder(embedBuilder, exception);
                else
                    embedBuilder.Description = FormatMessage(logEvent.RenderMessage(formatProvider), 240);

                (embedBuilder.Title, embedBuilder.Color) = GetEmbedLevel(logEvent.Level);
                
                if (includeProperties)
                {
                    foreach (var (key, value) in logEvent.Properties)
                        embedBuilder.AddField(key, FormatMessage(value.ToString(), MaxFieldLength));
                }

                webHook.SendMessageAsync(null, false, [embedBuilder.Build()])
                       .GetAwaiter()
                       .GetResult();
            }
            catch (Exception ex)
            {
                webHook.SendMessageAsync($"ooo snap, {ex.Message}")
                       .GetAwaiter()
                       .GetResult();
            }
        }

        private static (string title, Color color) GetEmbedLevel(LogEventLevel level) => level switch
        {
            LogEventLevel.Verbose     => (":loud_sound: Verbose", Color.LightGrey),
            LogEventLevel.Debug       => (":mag: Debug", Color.LightGrey),
            LogEventLevel.Information => (":information_source: Information", new(0, 186, 255)),
            LogEventLevel.Warning     => (":warning: Warning", new(255, 204, 0)),
            LogEventLevel.Error       => (":x: Error", new(255, 0, 0)),
            LogEventLevel.Fatal       => (":skull_crossbones: Fatal", Color.DarkRed),
            _                         => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };

        private static string FormatMessage(string message, int maxLength)
        {
            if (message.Length > maxLength)
                message = $"{message[..maxLength]} ...";

            if (!string.IsNullOrWhiteSpace(message))
                message = $"```{message}```";

            return message;
        }

        private static bool ShouldLogMessage(LogEventLevel minimumLogEventLevel,
                                             LogEventLevel messageLogEventLevel) =>
            (int)messageLogEventLevel >= (int)minimumLogEventLevel;

        private static void AddExceptionToEmbedBuilder(EmbedBuilder embedBuilder, Exception exception)
        {
            var isInner = false;
            while (true)
            {
                var type = $"```{exception.GetType().FullName}```";
                embedBuilder.AddField(isInner ? "Inner Exception Type:" : "Exception Type:", type);

                var message = FormatMessage(exception.Message, MaxFieldLength);
                embedBuilder.AddField(isInner ? "Inner Exception Message:" : "Exception Message:", message);

                if (exception.StackTrace is not null)
                {
                    var stackTrace = FormatMessage(exception.StackTrace, MaxFieldLength);
                    embedBuilder.AddField(isInner ? "Inner Exception StackTrace:" : "Exception StackTrace:", stackTrace);
                }

                if (exception.InnerException is not { } innerException) break;

                exception = innerException;
                isInner = true;
            }
        }
    }
}
