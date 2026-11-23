using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.Models;
using Social.Models.Enums;
using Social.Models.Threads;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot;

public class ThreadsPublisher(IThreadsSharer threadsSharer) : SocialPublisherBase
{
    private const int MaxTitleLength = 300;

    public override SocialMedia Platform => SocialMedia.Threads;

    public override async Task<bool> Publish(
        PostToShare post,
        EnvironmentVariables environmentVars,
        SocialMedia sourceSocial)
    {
        if (!IsPublisherEnabled(environmentVars))
        {
            return false;
        }

        var accessToken = environmentVars.ThreadsParams?.AccessToken;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Console.WriteLine($"Skipping {Platform}: THREADS_ACCESS_TOKEN is missing.");
            return false;
        }

        var threadsRequest = sourceSocial == SocialMedia.Instagram
            ? BuildInstagramRequest(accessToken, post)
            : BuildLinkPreviewRequest(accessToken, post);

        var response = await threadsSharer.SharePost(threadsRequest);

        return response?.IsSuccess
            ?? false;
    }

    private static ThreadsPostRequest BuildInstagramRequest(string accessToken, PostToShare post)
    {
        var imageUrl = string.IsNullOrWhiteSpace(post.ImageUrl) ? null : post.ImageUrl;

        return new(accessToken, BuildText(post), ImageUrl: imageUrl);
    }

    private static ThreadsPostRequest BuildLinkPreviewRequest(string accessToken, PostToShare post)
    {
        var linkUrl = string.IsNullOrWhiteSpace(post.Url) ? null : post.Url;

        return new(accessToken, BuildText(post), linkUrl);
    }

    private static string BuildText(PostToShare post)
    {
        var title = BuildTitle(post.Title);

        if (string.IsNullOrWhiteSpace(title))
        {
            return post.Url;
        }

        if (string.IsNullOrWhiteSpace(post.Url))
        {
            return title;
        }

        return $"{title}\n\n{post.Url}";
    }

    private static string BuildTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        var firstLine = title.Split('\n')[0].Trim();

        if (firstLine.Length <= MaxTitleLength)
        {
            return firstLine;
        }

        return $"{firstLine[..(MaxTitleLength - 1)]}…";
    }
}
