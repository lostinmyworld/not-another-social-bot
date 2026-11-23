using Moq;
using NotAnotherSocialBot.Models;
using Social.Models.Enums;
using Social.Models.Threads;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot.Tests;

public class ThreadsPublisherTests
{
    private const string Url = "https://example.com/post";

    [Fact]
    public async Task Publish_Disabled_ReturnsFalseWithoutSharing()
    {
        var environmentVars = TestEnvironment.Create();

        var (isPublished, request) = await PublishAndCaptureRequest(new PostToShare(Url, "title"), environmentVars);

        Assert.False(isPublished);
        Assert.Null(request);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Publish_MissingToken_ReturnsFalseWithoutSharing(string? threadsToken)
    {
        var environmentVars = CreateEnabledEnvironment(threadsToken);

        var (isPublished, request) = await PublishAndCaptureRequest(new PostToShare(Url, "title"), environmentVars);

        Assert.False(isPublished);
        Assert.Null(request);
    }

    [Fact]
    public async Task Publish_SharerSucceeds_ReturnsTrue()
    {
        var (isPublished, _) = await PublishAndCaptureRequest(new PostToShare(Url, "title"), CreateEnabledEnvironment());

        Assert.True(isPublished);
    }

    [Fact]
    public async Task Publish_SharerFails_ReturnsFalse()
    {
        var (isPublished, _) = await PublishAndCaptureRequest(
            new PostToShare(Url, "title"),
            CreateEnabledEnvironment(),
            new ThreadsPostResponse(false, ErrorMessage: "error"));

        Assert.False(isPublished);
    }

    [Fact]
    public async Task Publish_WithTitleAndUrl_SendsTitleThenUrlWithLinkPreview()
    {
        var (_, request) = await PublishAndCaptureRequest(new PostToShare(Url, "My video"), CreateEnabledEnvironment());

        Assert.Equal("threads-token", request?.AccessToken);
        Assert.Equal($"My video\n\n{Url}", request?.Text);
        Assert.Equal(Url, request?.LinkUrl);
    }

    [Fact]
    public async Task Publish_WithMultiLineTitle_SendsFirstLineOnly()
    {
        var (_, request) = await PublishAndCaptureRequest(new PostToShare(Url, "First line\nSecond line"), CreateEnabledEnvironment());

        Assert.Equal($"First line\n\n{Url}", request?.Text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Publish_WithoutTitle_SendsUrlOnly(string? title)
    {
        var (_, request) = await PublishAndCaptureRequest(new PostToShare(Url, title), CreateEnabledEnvironment());

        Assert.Equal(Url, request?.Text);
    }

    [Fact]
    public async Task Publish_WithoutUrl_SendsTitleOnlyWithoutLinkPreview()
    {
        var (_, request) = await PublishAndCaptureRequest(new PostToShare(string.Empty, "title"), CreateEnabledEnvironment());

        Assert.Equal("title", request?.Text);
        Assert.Null(request?.LinkUrl);
    }

    [Fact]
    public async Task Publish_WithTitleOf300Characters_SendsTitleUnchanged()
    {
        var title = new string('a', 300);

        var (_, request) = await PublishAndCaptureRequest(new PostToShare(Url, title), CreateEnabledEnvironment());

        Assert.Equal($"{title}\n\n{Url}", request?.Text);
    }

    [Fact]
    public async Task Publish_WithTitleOver300Characters_SendsTitleCutTo300()
    {
        var title = new string('a', 301);

        var (_, request) = await PublishAndCaptureRequest(new PostToShare(Url, title), CreateEnabledEnvironment());

        Assert.Equal($"{new string('a', 299)}…\n\n{Url}", request?.Text);
    }

    private static EnvironmentVariables CreateEnabledEnvironment(string? threadsToken = "threads-token")
        => TestEnvironment.Create(
            enabledPublishers: new() { { SocialMedia.Threads, true } },
            threadsToken: threadsToken);

    private static async Task<(bool IsPublished, ThreadsPostRequest? Request)> PublishAndCaptureRequest(
        PostToShare post,
        EnvironmentVariables environmentVars,
        ThreadsPostResponse? sharerResponse = null)
    {
        ThreadsPostRequest? sentRequest = null;

        var threadsSharer = new Mock<IThreadsSharer>();
        threadsSharer
            .Setup(sharer => sharer.SharePost(It.IsAny<ThreadsPostRequest>()))
            .Callback<ThreadsPostRequest>(request => sentRequest = request)
            .ReturnsAsync(sharerResponse ?? new ThreadsPostResponse(true, PostId: "post-1"));

        var publisher = new ThreadsPublisher(threadsSharer.Object);

        var isPublished = await publisher.Publish(post, environmentVars, SocialMedia.YouTube);

        return (isPublished, sentRequest);
    }
}
