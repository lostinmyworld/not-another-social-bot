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

    [Fact]
    public async Task Share_WatcherDisabledWithoutSettings_SkipsWithoutError()
    {
        var lastState = new LastState();
        var watcher = CreateWatcher(true);
        var environmentVars = TestEnvironment.Create(youTubeChannelId: null, youTubeApiKey: null);

        await watcher.Share(lastState, environmentVars);

        Assert.Empty(lastState.LastShared);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Share_ChannelIdMissing_Throws(string? channelId)
    {
        var watcher = CreateWatcher(true);
        var environmentVars = TestEnvironment.Create(
            enabledWatchers: new() { { SocialMedia.YouTube, true } },
            youTubeChannelId: channelId);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => watcher.Share(new LastState(), environmentVars));

        Assert.Equal("YOUTUBE_CHANNEL_ID is missing.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Share_ApiKeyMissing_Throws(string? apiKey)
    {
        var watcher = CreateWatcher(true);
        var environmentVars = TestEnvironment.Create(
            enabledWatchers: new() { { SocialMedia.YouTube, true } },
            youTubeApiKey: apiKey);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => watcher.Share(new LastState(), environmentVars));

        Assert.Equal("YOUTUBE_API_KEY is missing.", exception.Message);
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
                        It.IsAny<PostToShare>(),
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
