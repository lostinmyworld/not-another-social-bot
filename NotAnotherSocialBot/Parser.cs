using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.Models;
using Social.Models.Enums;
using Social.Models.Gist;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot;

public class Parser : IParser
{
    private readonly EnvironmentVariables _environmentVars;

    public Parser(IEnvironmentLoader environmentLoader)
    {
        var gistOptionsToRetrieve = new GistOptions()
        {
            GistId = "GIST_ID",
            GistToken = "GIST_TOKEN",
            GistStateFileName = "GIST_STATE_FILE_NAME",
        };
        var gistOptions = environmentLoader.LoadGistOptions(gistOptionsToRetrieve)!;

        _environmentVars = RetrieveEnvironmentVariables(gistOptions);
    }

    public EnvironmentVariables GetEnvironmentVariables()
    {
        return _environmentVars;
    }

    private static EnvironmentVariables RetrieveEnvironmentVariables(GistOptions gistOptions)
    {
        var enabledWatchers = new Dictionary<SocialMedia, bool>
        {
            { SocialMedia.YouTube, RetrieveFeatureFlagFromEnvironment("WATCHER_YOUTUBE_ENABLED") },
            { SocialMedia.X, RetrieveFeatureFlagFromEnvironment("WATCHER_TWITTER_ENABLED") },
        };
        var enabledPublishers = new Dictionary<SocialMedia, bool>
        {
            { SocialMedia.Discord, RetrieveFeatureFlagFromEnvironment("PUBLISHER_DISCORD_ENABLED") },
            { SocialMedia.X, RetrieveFeatureFlagFromEnvironment("PUBLISHER_TWITTER_ENABLED") },
        };

        var youTubeChannelId = Environment.GetEnvironmentVariable("YOUTUBE_CHANNEL_ID")!;
        var youTubeApiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY")!;

        var discordParams = new Dictionary<SocialMedia, string>
        {
            { SocialMedia.YouTube, Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_YOUTUBE")! },
        };

        var twitterParams = new TwitterParameters(
            Environment.GetEnvironmentVariable("TWITTER_CONSUMER_KEY")!,
            Environment.GetEnvironmentVariable("TWITTER_CONSUMER_SECRET")!,
            Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN")!,
            Environment.GetEnvironmentVariable("TWITTER_ACCESS_SECRET")!);

        var threadParams = new ThreadsParameters(
            Environment.GetEnvironmentVariable("THREADS_APP_ID")!,
            Environment.GetEnvironmentVariable("THREADS_ACCESS_TOKEN")!);

        return new(
            gistOptions,
            enabledWatchers,
            enabledPublishers,
            youTubeChannelId,
            youTubeApiKey,
            discordParams,
            twitterParams,
            threadParams);
    }

    private static bool RetrieveFeatureFlagFromEnvironment(string value)
    {
        var isEnabledStr = Environment.GetEnvironmentVariable(value);

        return bool.TryParse(isEnabledStr, out var isEnabled)
            && isEnabled;
    }
}
