using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.Models;
using Social.Models.Enums;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot;

public class YouTubeWatcher(
    IYouTubeConsumer youTubeConsumer,
    IEnumerable<ISocialPublisher> socialPublishers,
    IGistConsumer gistConsumer) : SocialWatcherBase
{
    public override SocialMedia Platform => SocialMedia.YouTube;

    public override async Task Share(
        LastState lastState,
        EnvironmentVariables environmentVars)
    {
        if (!IsWatcherEnabled(environmentVars))
        {
            Console.WriteLine($"Skipping {Platform}: WATCHER_YOUTUBE_ENABLED is not true.");
            return;
        }

        if (string.IsNullOrWhiteSpace(environmentVars.YouTubeChannelId))
        {
            throw new InvalidOperationException("YOUTUBE_CHANNEL_ID is missing.");
        }

        if (string.IsNullOrWhiteSpace(environmentVars.YouTubeApiKey))
        {
            throw new InvalidOperationException("YOUTUBE_API_KEY is missing.");
        }

        var lastShared = lastState.LastShared.GetValueOrDefault(Platform);
        var videoToShare = await youTubeConsumer.RetrieveOldestNotSharedVideo(
            environmentVars.YouTubeChannelId!,
            environmentVars.YouTubeApiKey,
            lastShared?.Id!,
            nameof(NotAnotherSocialBot));

        if (videoToShare is null
            || string.IsNullOrWhiteSpace(videoToShare.VideoId)
            || videoToShare.VideoId == lastShared?.Id)
        {
            Console.WriteLine($"No new videos to process. Exiting <{nameof(YouTubeWatcher)}.{nameof(Share)}>.");
            return;
        }

        Console.WriteLine($"New video detected: {videoToShare.Title} ({videoToShare.VideoId})");
        var videoUrl = $"https://www.youtube.com/watch?v={videoToShare.VideoId}";
        var postToShare = new PostToShare(videoUrl, videoToShare.Title);

        var isPublished = false;

        foreach (var publisher in socialPublishers)
        {
            var isPublishedByPublisher = await publisher.Publish(postToShare, environmentVars!, Platform);

            if (isPublishedByPublisher)
            {
                isPublished = true;
            }
        }

        if (!isPublished)
        {
            Console.WriteLine($"No publisher shared {videoToShare.VideoId}. State not saved.");
            return;
        }

        lastState.LastShared[SocialMedia.YouTube] = new(
            videoToShare.VideoId!,
            videoUrl,
            videoToShare.Title!,
            DateTime.Now);

        await gistConsumer.SaveCurrentState(lastState, environmentVars!.GistOptions);
    }
}
