using Social.Models.Enums;
using Social.Models.Gist;

namespace NotAnotherSocialBot.Models;

public record EnvironmentVariables(
    GistOptions GistOptions,
    IDictionary<SocialMedia, bool> EnabledWatchers,
    IDictionary<SocialMedia, bool> EnabledPublishers,
    string YouTubeChannelId,
    string YouTubeApiKey,
    IDictionary<SocialMedia, string> DiscordWebHooks,
    TwitterParameters TwitterParams,
    ThreadsParameters ThreadsParams);
