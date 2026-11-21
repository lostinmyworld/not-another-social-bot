using Moq;
using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.Models;
using Social.Models.Enums;
using Social.Models.YouTube;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot.Tests;

public class YouTubeWatcherTests
{
    private const string VideoId = "video-1";

    [Fact]
    public async Task Share_NoPublishers_DoesNotUpdateState()
    {
        var lastState = new LastState();
        var watcher = CreateWatcher();

        await watcher.Share(lastState, CreateEnvironmentVariables());

        Assert.Empty(lastState.LastShared);
    }

    [Fact]
    public async Task Share_AllPublishersFail_DoesNotUpdateState()
    {
        var lastState = new LastState();
        var watcher = CreateWatcher(false, false);

        await watcher.Share(lastState, CreateEnvironmentVariables());

        Assert.Empty(lastState.LastShared);
    }

    [Fact]
    public async Task Share_FirstPublisherSucceedsSecondFails_UpdatesState()
    {
        var lastState = new LastState();
        var watcher = CreateWatcher(true, false);

        await watcher.Share(lastState, CreateEnvironmentVariables());

        Assert.Equal(VideoId, lastState.LastShared[SocialMedia.YouTube].Id);
    }

    private static YouTubeWatcher CreateWatcher(params bool[] publishResults)
    {
        var youTubeConsumer = new Mock<IYouTubeConsumer>();
        youTubeConsumer
            .Setup(c => c.RetrieveOldestNotSharedVideo(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new YouTubeVideo { VideoId = VideoId, Title = "title" });

        var publishers = publishResults
            .Select(result =>
            {
                var publisher = new Mock<ISocialPublisher>();
                publisher
                    .Setup(p => p.Publish(
                        It.IsAny<string>(),
                        It.IsAny<EnvironmentVariables>(),
                        It.IsAny<SocialMedia>()))
                    .ReturnsAsync(result);

                return publisher.Object;
            })
            .ToList();

        return new YouTubeWatcher(
            youTubeConsumer.Object,
            publishers,
            new Mock<IGistConsumer>().Object);
    }

    private static EnvironmentVariables CreateEnvironmentVariables()
        => TestEnvironment.Create(enabledWatchers: new() { { SocialMedia.YouTube, true } });
}
