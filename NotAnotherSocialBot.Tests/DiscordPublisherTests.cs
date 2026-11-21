using Moq;
using NotAnotherSocialBot.Models;
using Social.Models.Discord;
using Social.Models.Enums;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot.Tests;

public class DiscordPublisherTests
{
    private const string Url = "https://example.com/post";

    [Theory]
    [InlineData(SocialMedia.YouTube)]
    [InlineData(SocialMedia.Discord)]
    [InlineData(SocialMedia.X)]
    [InlineData(SocialMedia.Facebook)]
    [InlineData(SocialMedia.TikTok)]
    public async Task Publish_NotFromInstagram_SendsYouTubeMessage(SocialMedia sourceSocial)
    {
        var sentContent = await PublishAndCaptureContent(Url, sourceSocial);

        Assert.Equal($"\n\n**New YouTube Video @everyone !!**\n{Url}", sentContent);
    }

    [Fact]
    public async Task Publish_FromInstagram_SendsBoldHeaderThenUrl()
    {
        var sentContent = await PublishAndCaptureContent(Url, SocialMedia.Instagram);

        var lines = sentContent!.Split('\n');
        Assert.Equal(2, lines.Length);
        Assert.StartsWith("**", lines[0]);
        Assert.EndsWith("**", lines[0]);
        Assert.Equal(Url, lines[1]);
    }

    [Fact]
    public async Task Publish_FromInstagramWithoutUrl_SendsHeaderOnly()
    {
        var sentContent = await PublishAndCaptureContent(string.Empty, SocialMedia.Instagram);

        Assert.DoesNotContain("\n", sentContent);
        Assert.StartsWith("**", sentContent);
    }

    private static async Task<string?> PublishAndCaptureContent(string url, SocialMedia sourceSocial)
    {
        string? sentContent = null;

        var discordSharer = new Mock<IDiscordSharer>();
        discordSharer
            .Setup(sharer => sharer.SendToDiscord(It.IsAny<DiscordRequest>(), It.IsAny<string>()))
            .Callback<DiscordRequest, string>((request, _) => sentContent = request.Content)
            .Returns(Task.CompletedTask);

        var publisher = new DiscordPublisher(discordSharer.Object);
        var environmentVars = TestEnvironment.Create(
            enabledPublishers: new() { { SocialMedia.Discord, true } },
            discordWebHooks: new() { { sourceSocial, "https://discord.example/webhook" } });

        await publisher.Publish(url, environmentVars, sourceSocial);

        return sentContent;
    }
}
