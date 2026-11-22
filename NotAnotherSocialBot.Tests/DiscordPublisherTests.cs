using Moq;
using NotAnotherSocialBot.Models;
using Social.Models.Discord;
using Social.Models.Enums;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot.Tests;

public class DiscordPublisherTests
{
    private const string Url = "https://example.com/post";
    private const string ImageUrl = "https://cdn.example/image.jpg";

    [Theory]
    [InlineData(SocialMedia.YouTube)]
    [InlineData(SocialMedia.Discord)]
    [InlineData(SocialMedia.X)]
    [InlineData(SocialMedia.Facebook)]
    [InlineData(SocialMedia.TikTok)]
    public async Task Publish_NotFromInstagram_SendsYouTubeMessageWithoutEmbed(SocialMedia sourceSocial)
    {
        var request = await PublishAndCaptureRequest(new PostToShare(Url, "title", ImageUrl), sourceSocial);

        Assert.Equal($"\n\n**New YouTube Video @everyone !!**\n{Url}", request?.Content);
        Assert.Empty(request!.Embeds);
    }

    [Fact]
    public async Task Publish_FromInstagram_SendsBoldHeaderOnlyAsContent()
    {
        var request = await PublishAndCaptureRequest(new PostToShare(Url), SocialMedia.Instagram);

        Assert.DoesNotContain("\n", request?.Content);
        Assert.StartsWith("**", request?.Content);
        Assert.EndsWith("**", request?.Content);
    }

    [Fact]
    public async Task Publish_FromInstagram_EmbedLinksToPost()
    {
        var embed = await PublishInstagramAndCaptureEmbed(new PostToShare(Url));

        Assert.Equal(Url, embed["url"]);
    }

    [Fact]
    public async Task Publish_FromInstagramWithoutUrl_EmbedHasNoLink()
    {
        var embed = await PublishInstagramAndCaptureEmbed(new PostToShare(string.Empty));

        Assert.False(embed.ContainsKey("url"));
    }

    [Fact]
    public async Task Publish_FromInstagramWithImage_EmbedShowsImage()
    {
        var embed = await PublishInstagramAndCaptureEmbed(new PostToShare(Url, ImageUrl: ImageUrl));

        var image = Assert.IsType<Dictionary<string, object?>>(embed["image"]);
        Assert.Equal(ImageUrl, image["url"]);
    }

    [Fact]
    public async Task Publish_FromInstagramWithoutImage_EmbedHasNoImage()
    {
        var embed = await PublishInstagramAndCaptureEmbed(new PostToShare(Url));

        Assert.False(embed.ContainsKey("image"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Publish_FromInstagramWithoutCaption_EmbedTitleIsDefault(string? caption)
    {
        var embed = await PublishInstagramAndCaptureEmbed(new PostToShare(Url, caption));

        Assert.Equal("View on Instagram", embed["title"]);
    }

    [Fact]
    public async Task Publish_FromInstagramWithMultiLineCaption_EmbedTitleIsFirstLine()
    {
        var embed = await PublishInstagramAndCaptureEmbed(new PostToShare(Url, "First line\nSecond line"));

        Assert.Equal("First line", embed["title"]);
    }

    [Fact]
    public async Task Publish_FromInstagramWithCaptionOf256Characters_EmbedTitleIsUnchanged()
    {
        var caption = new string('a', 256);

        var embed = await PublishInstagramAndCaptureEmbed(new PostToShare(Url, caption));

        Assert.Equal(caption, embed["title"]);
    }

    [Fact]
    public async Task Publish_FromInstagramWithCaptionOver256Characters_EmbedTitleIsCutTo256()
    {
        var caption = new string('a', 300);

        var embed = await PublishInstagramAndCaptureEmbed(new PostToShare(Url, caption));

        Assert.Equal($"{new string('a', 255)}…", embed["title"]);
    }

    private static async Task<Dictionary<string, object?>> PublishInstagramAndCaptureEmbed(PostToShare post)
    {
        var request = await PublishAndCaptureRequest(post, SocialMedia.Instagram);

        return Assert.Single(request!.Embeds);
    }

    private static async Task<DiscordRequest?> PublishAndCaptureRequest(PostToShare post, SocialMedia sourceSocial)
    {
        DiscordRequest? sentRequest = null;

        var discordSharer = new Mock<IDiscordSharer>();
        discordSharer
            .Setup(sharer => sharer.SendToDiscord(It.IsAny<DiscordRequest>(), It.IsAny<string>()))
            .Callback<DiscordRequest, string>((request, _) => sentRequest = request)
            .Returns(Task.CompletedTask);

        var publisher = new DiscordPublisher(discordSharer.Object);
        var environmentVars = TestEnvironment.Create(
            enabledPublishers: new() { { SocialMedia.Discord, true } },
            discordWebHooks: new() { { sourceSocial, "https://discord.example/webhook" } });

        await publisher.Publish(post, environmentVars, sourceSocial);

        return sentRequest;
    }
}
