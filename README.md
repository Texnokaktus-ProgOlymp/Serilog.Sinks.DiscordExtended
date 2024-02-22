Forked from [serilog-contrib/Serilog-Sinks-Discord](https://github.com/serilog-contrib/Serilog-Sinks-Discord)

# Serilog sink for Discord

### Write your logs to discord.

 #### To get started:
 :one:: Get ```WebhookId``` and ```WebhookToken``` </br>
> [Create webhoook](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks) and copy its url
which contains WebhookId and WebhookToken: </br>
```https://discordapp.com/api/webhooks/{WebhookId}/{WebhookToken}```

:two:: Install [nuget package]()

:three:: Add discord output: </br>
 ```csharp
Log.Logger = new LoggerConfiguration()
  .WriteTo.Discord({WebhookId}, {WebhookToken})
  .CreateLogger();
```
