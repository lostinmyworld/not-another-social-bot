using Social.Models.Instagram;

namespace NotAnotherSocialBot.Models;

public record PostToShare(
    string Url,
    string? Title = null,
    string? ImageUrl = null)
{
    private const string InstagramVideoType = "VIDEO";

    public static PostToShare FromInstagram(InstagramPost post)
    {
        var isVideo = post.MediaType == InstagramVideoType;
        var imageUrl = isVideo
            ? post.ThumbnailUrl
            : post.MediaUrl;

        return new(
            post.Permalink ?? string.Empty,
            post.Caption,
            imageUrl);
    }
}
