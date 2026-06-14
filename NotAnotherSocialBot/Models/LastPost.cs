namespace NotAnotherSocialBot.Models;

public record LastPost(
    string Id,
    string UrlToShare,
    string Title,
    DateTime DetectedAt);
