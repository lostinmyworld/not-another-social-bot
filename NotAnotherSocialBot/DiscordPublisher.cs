using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.Models;
using Social.Models.Discord;
using Social.Models.Enums;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot;

public class DiscordPublisher(IDiscordSharer discordSharer) : SocialPublisherBase
{
    private const string DefaultInstagramTitle = "View on Instagram";
    private const int MaxEmbedTitleLength = 256;

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
        PostToShare post,
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

            var discordRequest = sourceSocial == SocialMedia.Instagram
                ? BuildInstagramRequest(post)
                : BuildYouTubeRequest(post);

            await discordSharer.SendToDiscord(discordRequest, discordWebHook);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{nameof(DiscordPublisher)} failed with message: {ex.Message}.");
        }

        return false;
    }

    private static DiscordRequest BuildYouTubeRequest(PostToShare post)
    {
        return new()
        {
            Content = $"\n\n**New YouTube Video @everyone !!**\n{post.Url}",
        };
    }

    private static DiscordRequest BuildInstagramRequest(PostToShare post)
    {
        var header = _instagramHeaders[Random.Shared.Next(_instagramHeaders.Length)];

        return new()
        {
            Content = header,
            Embeds = [BuildInstagramEmbed(post)],
        };
    }

    private static Dictionary<string, object?> BuildInstagramEmbed(PostToShare post)
    {
        var embed = new Dictionary<string, object?>
        {
            ["title"] = BuildEmbedTitle(post.Title),
        };

        if (!string.IsNullOrWhiteSpace(post.Url))
        {
            embed["url"] = post.Url;
        }

        if (!string.IsNullOrWhiteSpace(post.ImageUrl))
        {
            embed["image"] = new Dictionary<string, object?> { ["url"] = post.ImageUrl };
        }

        return embed;
    }

    private static string BuildEmbedTitle(string? caption)
    {
        if (string.IsNullOrWhiteSpace(caption))
        {
            return DefaultInstagramTitle;
        }

        var firstLine = caption.Split('\n')[0].Trim();

        if (firstLine.Length <= MaxEmbedTitleLength)
        {
            return firstLine;
        }

        return $"{firstLine[..(MaxEmbedTitleLength - 1)]}…";
    }
}
