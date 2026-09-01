using System.Collections.Concurrent;
using NetCord.Rest;


public class webhookCacheService
{
    private readonly ConcurrentDictionary<ulong, IncomingWebhook> webhooks = new();

    public async Task<IncomingWebhook> getOrCreateWebhookAsync(RestClient rest, ulong channelId)
    {
        if(webhooks.TryGetValue(channelId, out var cached))
            return cached;

        var channelWebhooks = await rest.GetChannelWebhooksAsync(channelId);
        var webhook = channelWebhooks.OfType<IncomingWebhook>().FirstOrDefault(w => w.Name == "Deadbeat")
            ?? await rest.CreateWebhookAsync(channelId, new WebhookProperties("Deadbeat"));

            webhooks[channelId] = webhook;
            return webhook;
        
    }
}