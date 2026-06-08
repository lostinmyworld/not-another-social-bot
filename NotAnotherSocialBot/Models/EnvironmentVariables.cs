using Social.Models.Gist;

namespace NotAnotherSocialBot.Models;

public record EnvironmentVariables(
    GistOptions GistOptions,
    string YouTubeChannelId);
