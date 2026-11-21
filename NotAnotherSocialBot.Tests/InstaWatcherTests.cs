using Moq;
using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.Models;
using Social.Models.Enums;
using Social.Models.Instagram;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot.Tests;

public class InstaWatcherTests
{
    private static readonly List<InstagramPost> _postsNewestFirst =
    [
        new() { Id = "newest", Permalink = "https://instagram.example/newest" },
        new() { Id = "middle", Permalink = "https://instagram.example/middle" },
        new() { Id = "oldest", Permalink = "https://instagram.example/oldest" },
    ];

    [Fact]
    public async Task Share_NoPosts_DoesNotUpdateState()
    {
        var lastState = new LastState();
        var watcher = CreateWatcher([], publishResult: true);

        await watcher.Share(lastState, CreateEnvironmentVariables());

        Assert.Empty(lastState.LastShared);
    }

    [Fact]
    public async Task Share_NothingSharedYet_SharesOldestPost()
    {
        var lastState = new LastState();
        var watcher = CreateWatcher(_postsNewestFirst, publishResult: true);

        await watcher.Share(lastState, CreateEnvironmentVariables());

        Assert.Equal("oldest", lastState.LastShared[SocialMedia.Instagram].Id);
    }

    [Fact]
    public async Task Share_PostShared_SavesPermalinkAsUrl()
    {
        var lastState = new LastState();
        var watcher = CreateWatcher(_postsNewestFirst, publishResult: true);

        await watcher.Share(lastState, CreateEnvironmentVariables());

        Assert.Equal("https://instagram.example/oldest", lastState.LastShared[SocialMedia.Instagram].UrlToShare);
    }

    [Fact]
    public async Task Share_LastSharedIsOldest_SharesNextNewer()
    {
        var lastState = CreateStateWithLastShared("oldest");
        var watcher = CreateWatcher(_postsNewestFirst, publishResult: true);

        await watcher.Share(lastState, CreateEnvironmentVariables());

        Assert.Equal("middle", lastState.LastShared[SocialMedia.Instagram].Id);
    }

    [Fact]
    public async Task Share_LastSharedIsNewest_KeepsState()
    {
        var lastState = CreateStateWithLastShared("newest");
        var watcher = CreateWatcher(_postsNewestFirst, publishResult: true);

        await watcher.Share(lastState, CreateEnvironmentVariables());

        Assert.Equal("newest", lastState.LastShared[SocialMedia.Instagram].Id);
    }

    [Fact]
    public async Task Share_LastSharedNotInPosts_KeepsState()
    {
        var lastState = CreateStateWithLastShared("deleted");
        var watcher = CreateWatcher(_postsNewestFirst, publishResult: true);

        await watcher.Share(lastState, CreateEnvironmentVariables());

        Assert.Equal("deleted", lastState.LastShared[SocialMedia.Instagram].Id);
    }

    [Fact]
    public async Task Share_PublisherFails_DoesNotUpdateState()
    {
        var lastState = new LastState();
        var watcher = CreateWatcher(_postsNewestFirst, publishResult: false);

        await watcher.Share(lastState, CreateEnvironmentVariables());

        Assert.Empty(lastState.LastShared);
    }

    private static InstaWatcher CreateWatcher(List<InstagramPost> posts, bool publishResult)
    {
        var instagramConsumer = new Mock<IInstagramConsumer>();
        instagramConsumer
            .Setup(consumer => consumer.RetrievePostsAsync(It.IsAny<InstagramRequest>()))
            .ReturnsAsync(posts);

        var publisher = new Mock<ISocialPublisher>();
        publisher
            .Setup(p => p.Publish(
                It.IsAny<string>(),
                It.IsAny<EnvironmentVariables>(),
                It.IsAny<SocialMedia>()))
            .ReturnsAsync(publishResult);

        return new InstaWatcher(
            instagramConsumer.Object,
            [publisher.Object],
            new Mock<IGistConsumer>().Object);
    }

    private static LastState CreateStateWithLastShared(string postId)
    {
        var lastState = new LastState();
        lastState.LastShared[SocialMedia.Instagram] = new(postId, "url", "caption", DateTime.Now);

        return lastState;
    }

    private static EnvironmentVariables CreateEnvironmentVariables()
        => TestEnvironment.Create(enabledWatchers: new() { { SocialMedia.Instagram, true } });
}
