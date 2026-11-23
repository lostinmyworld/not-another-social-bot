using NotAnotherSocialBot.Models;
using Social.Models.Enums;
using Social.Models.Gist;

namespace NotAnotherSocialBot.Tests;

public static class TestEnvironment
{
    public static EnvironmentVariables Create(
        Dictionary<SocialMedia, bool>? enabledWatchers = null,
        Dictionary<SocialMedia, bool>? enabledPublishers = null,
        Dictionary<SocialMedia, string>? discordWebHooks = null,
        string? youTubeChannelId = "channel-id",
        string? youTubeApiKey = "api-key",
        string? instagramToken = "instagram-token",
        string? threadsToken = "threads-token")
        => new(
            new GistOptions(),
            enabledWatchers ?? [],
            enabledPublishers ?? [],
            youTubeChannelId!,
            youTubeApiKey!,
            instagramToken!,
            discordWebHooks ?? [],
            new TwitterParameters("", "", "", ""),
            new ThreadsParameters("", threadsToken!));
}
