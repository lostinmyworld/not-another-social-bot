using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.Models;
using Social.Models.Discord;
using Social.Models.Enums;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot;

public class DiscordPublisher(IDiscordSharer discordSharer) : SocialPublisherBase
{
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
                Content = $"\n\n**New YouTube Video @everyone !!**\n{url}",
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
}
