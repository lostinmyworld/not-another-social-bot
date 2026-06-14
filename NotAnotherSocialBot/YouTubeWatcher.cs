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
        if (environmentVars is null
            || string.IsNullOrWhiteSpace(environmentVars.YouTubeChannelId))
        {
            return;
        }

        if (!IsWatcherEnabled(environmentVars))
        {
            return;
        }

        var lastShared = lastState.LastShared.GetValueOrDefault(Platform);
        var videoToShare = await youTubeConsumer.RetrieveOldestNotSharedVideo(
            environmentVars?.YouTubeChannelId!,
            lastShared?.Id!);

        if (videoToShare is null
            || string.IsNullOrWhiteSpace(videoToShare.VideoId)
            || videoToShare.VideoId == lastShared?.Id)
        {
            Console.WriteLine($"No new videos to process. Exiting <{nameof(YouTubeWatcher)}.{nameof(Share)}>.");
            return;
        }

        Console.WriteLine($"New video detected: {videoToShare.Title} ({videoToShare.VideoId})");
        var videoUrl = $"https://www.youtube.com/watch?v={videoToShare.VideoId}";

        foreach (var publisher in socialPublishers)
        {
            await publisher.Publish(videoUrl, environmentVars!, Platform);
        }

        lastState.LastShared[SocialMedia.YouTube] = new(
            videoToShare.VideoId!,
            videoUrl,
            videoToShare.Title!,
            DateTime.Now);

        await gistConsumer.SaveCurrentState(lastState, environmentVars!.GistOptions);
    }
}
