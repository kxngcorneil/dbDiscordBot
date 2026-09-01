using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

var builder = Host.CreateApplicationBuilder(args);



builder.Services.AddDiscordGateway(options =>
{
    options.Token = builder.Configuration["Discord:Token"]
        ?? throw new InvalidOperationException("No token");

    options.Intents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent;
});

builder.Services.AddSingleton<webhookCacheService>();
builder.Services.AddGatewayHandlers(typeof(Program).Assembly);
var host = builder.Build();

Console.WriteLine("Starting connection with bot");
await host.RunAsync();