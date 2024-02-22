using Discord;
using Discord.Webhook;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.DiscordExtended
{
    public class DiscordSink(IFormatProvider? formatProvider,
                             ulong webhookId,
                             string webhookToken,
                             LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information) : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            SendMessage(logEvent);
        }

        private void SendMessage(LogEvent logEvent)
        {
            if (!ShouldLogMessage(restrictedToMinimumLevel, logEvent.Level))
                return;

            var embedBuilder = new EmbedBuilder();
            var webHook = new DiscordWebhookClient(webhookId, webhookToken);

            try
            {
                if (logEvent.Exception != null)
                {
                    embedBuilder.Color = new Color(255, 0, 0);
                    embedBuilder.WithTitle(":o: Exception");
                    embedBuilder.AddField("Type:", $"```{logEvent.Exception.GetType().FullName}```");

                    var message = FormatMessage(logEvent.Exception.Message, 1000);
                    embedBuilder.AddField("Message:", message);

                    if (logEvent.Exception.StackTrace is not null)
                    {
                        var stackTrace = FormatMessage(logEvent.Exception.StackTrace, 1000);
                        embedBuilder.AddField("StackTrace:", stackTrace);
                    }

                    webHook.SendMessageAsync(null, false, [embedBuilder.Build()])
                           .GetAwaiter()
                           .GetResult();
                }
                else
                {
                    var message = logEvent.RenderMessage(formatProvider);

                    message = FormatMessage(message, 240);

                    (embedBuilder.Title, embedBuilder.Color) = GetEmbedLevel(logEvent.Level);

                    embedBuilder.Description = message;

                    webHook.SendMessageAsync(null, false, [embedBuilder.Build()])
                           .GetAwaiter()
                           .GetResult();
                }
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
    }
}
