using NotAnotherSocialBot.Models;
using Social.Models.Instagram;

namespace NotAnotherSocialBot.Tests;

public class PostToShareTests
{
    private const string MediaUrl = "https://cdn.example/media";
    private const string ThumbnailUrl = "https://cdn.example/thumbnail.jpg";

    [Fact]
    public void FromInstagram_Video_UsesThumbnailAsImage()
    {
        var post = CreateInstagramPost("VIDEO");

        var result = PostToShare.FromInstagram(post);

        Assert.Equal(ThumbnailUrl, result.ImageUrl);
    }

    [Theory]
    [InlineData("IMAGE")]
    [InlineData("CAROUSEL_ALBUM")]
    [InlineData(null)]
    public void FromInstagram_NotVideo_UsesMediaUrlAsImage(string? mediaType)
    {
        var post = CreateInstagramPost(mediaType);

        var result = PostToShare.FromInstagram(post);

        Assert.Equal(MediaUrl, result.ImageUrl);
    }

    [Fact]
    public void FromInstagram_Post_UsesPermalinkAsUrlAndCaptionAsTitle()
    {
        var post = CreateInstagramPost("IMAGE");

        var result = PostToShare.FromInstagram(post);

        Assert.Equal("https://instagram.example/p/1", result.Url);
        Assert.Equal("caption", result.Title);
    }

    [Fact]
    public void FromInstagram_NoPermalink_UsesEmptyUrl()
    {
        var post = CreateInstagramPost("IMAGE");
        post.Permalink = null;

        var result = PostToShare.FromInstagram(post);

        Assert.Equal(string.Empty, result.Url);
    }

    private static InstagramPost CreateInstagramPost(string? mediaType)
        => new()
        {
            Id = "1",
            MediaType = mediaType,
            MediaUrl = MediaUrl,
            ThumbnailUrl = ThumbnailUrl,
            Permalink = "https://instagram.example/p/1",
            Caption = "caption",
        };
}
