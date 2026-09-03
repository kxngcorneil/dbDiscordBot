using System.Security.AccessControl;
using System.Text.RegularExpressions;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace DeadbeatBot;

public class messageCreatedHandle : IMessageCreateGatewayHandler
{
    private static readonly Regex SocialMediaRegex = new(
    @"https?://(?:www\.)?(?<!fx|vx|dd|rx)(?:x\.com|twitter\.com|tiktok\.com|instagram\.com|reddit\.com)/\S+",
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
        if (message.Author.IsBot)
            return;

        if (message.Channel is null)
            return;

        if (!SocialMediaRegex.IsMatch(message.Content))
            return;

        string fixedUrl = SocialMediaRegex.Replace(
    message.Content,
    match => match.Value
        .Replace("://x.com", "://fxtwitter.com")
        .Replace("://www.x.com", "://fxtwitter.com")
        .Replace("://twitter.com", "://fxtwitter.com")
        .Replace("://www.twitter.com", "://fxtwitter.com")
        .Replace("://tiktok.com", "://vxtiktok.com")
        .Replace("://www.tiktok.com", "://vxtiktok.com")
        .Replace("://instagram.com", "://ddinstagram.com")
        .Replace("://www.instagram.com", "://ddinstagram.com")
        .Replace("://reddit.com", "://rxddit.com")
        .Replace("://www.reddit.com", "://rxddit.com")
);



        string authorName = message.Author.GlobalName ?? message.Author.Username;
        ImageUrl? avatarImageUrl = message.Author.GetAvatarUrl();
        string avatarUrl = avatarImageUrl?.ToString() ?? string.Empty;

        IncomingWebhook webhook = await _webhookCache.getOrCreateWebhookAsync(_restClient, message.ChannelId);
        using var webhookClient = new WebhookClient(webhook.Id, webhook.Token!);
        await webhookClient.ExecuteAsync(new WebhookMessageProperties{

            Content = fixedUrl,
            Username = authorName,
            AvatarUrl = avatarUrl
        });
        await message.DeleteAsync();

        Console.WriteLine($"[DETECTED] Link from {message.Author.Username}: {message.Content}");
        Console.WriteLine($"[FIXED OUTPUT]: {fixedUrl}");

    }
}