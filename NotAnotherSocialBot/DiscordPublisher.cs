using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.Models;
using Social.Models.Discord;
using Social.Models.Enums;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot;

public class DiscordPublisher(IDiscordSharer discordSharer) : SocialPublisherBase
{
    private static readonly string[] _instagramHeaders =
    [
        "**Το έχεις δει αυτό;**",
        "**Μήπως σου ξέφυγε αυτό;**",
        "**Κάτι νέο ανέβηκε 👀**",
        "**Νέο post στον αέρα 🚀**",
        "**Ρίξε μια ματιά σε αυτό 👇**",
        "**Τσι τσι ρι πο τσι ρι**",
        "**Notification; Δεν ήρθε; Σου το φέρνω εγώ.**",
        "**Αυτό αξίζει ένα κλικ.**"
    ];

    public override SocialMedia Platform => SocialMedia.Discord;

    public override async Task<bool> Publish(
        string url,
        EnvironmentVariables environmentVars,
        SocialMedia sourceSocial)
    {
        try
        {
            if (environmentVars is null
                || !environmentVars.DiscordWebHooks.TryGetValue(sourceSocial, out var discordWebHook)
                || string.IsNullOrWhiteSpace(discordWebHook))
            {
                return false;
            }

            if (!IsPublisherEnabled(environmentVars))
            {
                return false;
            }

            var discordRequest = new DiscordRequest
            {
                Content = BuildMessage(url, sourceSocial),
            };

            await discordSharer.SendToDiscord(discordRequest, discordWebHook);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{nameof(DiscordPublisher)} failed with message: {ex.Message}.");
        }

        return false;
    }

    private static string BuildMessage(string url, SocialMedia sourceSocial)
    {
        return sourceSocial switch
        {
            SocialMedia.Instagram => BuildInstagramMessage(url),
            _ => $"\n\n**New YouTube Video @everyone !!**\n{url}",
        };
    }

    private static string BuildInstagramMessage(string url)
    {
        var header = _instagramHeaders[Random.Shared.Next(_instagramHeaders.Length)];

        if (string.IsNullOrWhiteSpace(url))
        {
            return header;
        }

        return $"{header}\n{url}";
    }
}
