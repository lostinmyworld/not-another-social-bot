using NotAnotherSocialBot.Models;
using Social.Models.Enums;
using Social.Models.Gist;

namespace NotAnotherSocialBot.Tests;

public static class TestEnvironment
{
    public static EnvironmentVariables Create(
        Dictionary<SocialMedia, bool>? enabledWatchers = null,
        Dictionary<SocialMedia, bool>? enabledPublishers = null,
        Dictionary<SocialMedia, string>? discordWebHooks = null)
        => new(
            new GistOptions(),
            enabledWatchers ?? [],
            enabledPublishers ?? [],
            "channel-id",
            "api-key",
            "instagram-token",
            discordWebHooks ?? [],
            new TwitterParameters("", "", "", ""),
            new ThreadsParameters("", ""));
}
