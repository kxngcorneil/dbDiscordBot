using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

class Program
{
    //DiscordSocketCLient is basically the bot
    private DiscordSocketClient? client;
    private IConfiguration? config;
    private readonly Dictionary<ulong, IWebhook> webhooks = new();
    private readonly SemaphoreSlim webhookLock = new(1,1);

    static Task Main()
    {
        return new Program().MainAsync();
    }

   

    public async Task MainAsync()
    {
        config = new ConfigurationBuilder()
        //Load configuration from file 
        .AddJsonFile("appsettings.json")
        .Build();

        string token = config["Discord:Token"] ?? throw new InvalidOperationException("Discord token is missing.");
        client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = 
                 GatewayIntents.Guilds |
                GatewayIntents.GuildMessages |
                GatewayIntents.MessageContent
        });

        client.Log += LogAsync;
        client.Ready += ReadyAsync;

        client.MessageReceived += MessageReceivedAsync;

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

    

        //Keeps application alive to keep receiving events 
        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        Console.WriteLine("Bot is connected!");
        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(SocketMessage message)

  
    {
              if(message.Author.IsBot)    
        return;


        Console.WriteLine(
             $"[{message.Channel.Name}] {message.Author.Username}: {message.Content}");

              if(!XLinkRegex.IsMatch(message.Content))
              return;



        Console.WriteLine("X/Twitter link detected!");


        string fixedUrl = XLinkRegex.Replace(
            message.Content,
            match => match.Value
                   .Replace("x.com", "fxtwitter.com")
                    .Replace("twitter.com", "fxtwitter.com")
        );
        

        Console.WriteLine($"Fixed: {fixedUrl}");

    
       
        if(message.Channel is not SocketTextChannel channel)
        return;
        string username = message.Author.GlobalName ?? message.Author.Username;
        string? avatarUrl = message.Author.GetAvatarUrl();
        IWebhook webhook = await GetWebhookAsync(channel);


        await webhook.SendMessageAsync(text: fixedUrl, username: username, avatarUrl: avatarUrl);
        await message.DeleteAsync();
    }   

     private async Task<IWebhook> GetWebhookAsync(SocketTextChannel channel){
        if(webhooks.TryGetValue(channel.Id, out IWebhook? webhook)){
            return webhook;
        }

        IReadOnlyCollection<IWebhook> existingWebhooks = await channel.GetWebhooksAsync();

        IWebhook? existingWebhook = existingWebhooks
        .FirstOrDefault(webhook => webhook.Name == "Deadbeat");

        if(existingWebhook != null)
        {
            webhooks[channel.Id] = existingWebhook;
            return existingWebhook;
        }


        IWebhook newwWebhook = await channel.CreateWebhookAsync("Deadbeat");
        webhooks[channel.Id] = newwWebhook;
        return newwWebhook;
    }
    

    private static readonly Regex XLinkRegex =
        new Regex(
             @"https?://(?:www\.)?(?:x\.com|twitter\.com)/\S+",
             RegexOptions.IgnoreCase);
        

}