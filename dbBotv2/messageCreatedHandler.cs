using System.Text.RegularExpressions;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace DeadbeatBot;

public class messageCreatedHandle : IMessageCreateGatewayHandler
{
    private static readonly Regex XLinkRegex = new(
    @"https?://(?:www\.)?(?<!fx|vx)(?:x\.com|twitter\.com)/\S+",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly webhookCacheService _webhookCache;
    private readonly RestClient _restClient;

    public messageCreatedHandle(webhookCacheService webhookCache, RestClient restClient)
    {
        _webhookCache = webhookCache;
        _restClient = restClient;
    }
    
    public async ValueTask HandleAsync(Message message)
    {
        if(message.Author.IsBot)
            return;

        if(message.Channel is null)
            return;

        if(!XLinkRegex.IsMatch(message.Content))
            return;

        string fixedUrl = XLinkRegex.Replace(
    message.Content, 
    match => match.Value
        .Replace("://x.com", "://fxtwitter.com")
        .Replace("://www.x.com", "://fxtwitter.com")
        .Replace("://twitter.com", "://fxtwitter.com")
        .Replace("://www.twitter.com", "://fxtwitter.com")
);
        


        string authorName = message.Author.GlobalName ?? message.Author.Username;
        ImageUrl? avatarImageUrl = message.Author.GetAvatarUrl();
        string avatarUrl = avatarImageUrl?.ToString() ?? string.Empty;

        IncomingWebhook webhook = await _webhookCache.getOrCreateWebhookAsync(_restClient, message.ChannelId);

        await webhook.ExecuteAsync(new WebhookMessageProperties
        {
            Content = fixedUrl,
            Username = authorName,
            AvatarUrl = avatarUrl
        });
    await message.DeleteAsync();

        Console.WriteLine($"[DETECTED] Link from {message.Author.Username}: {message.Content}");
        Console.WriteLine($"[FIXED OUTPUT]: {fixedUrl}");

    }
}