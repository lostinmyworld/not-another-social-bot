using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.Models;
using Social.Models.Enums;
using Social.Models.Instagram;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot;

public class InstaWatcher(
    IInstagramConsumer instagramConsumer,
    IEnumerable<ISocialPublisher> socialPublishers,
    IGistConsumer gistConsumer) : SocialWatcherBase
{
    private const string UserAgent = "not-another-social-bot/1.0";
    private const int PostsToFetch = 100;

    public override SocialMedia Platform => SocialMedia.Instagram;

    public override async Task Share(
        LastState lastState,
        EnvironmentVariables environmentVars)
    {
        if (environmentVars is null
            || string.IsNullOrWhiteSpace(environmentVars.InstagramToken))
        {
            return;
        }

        if (!IsWatcherEnabled(environmentVars))
        {
            return;
        }

        var instagramRequest = new InstagramRequest(environmentVars.InstagramToken, UserAgent, PostsToFetch);
        var postsNewestFirst = await instagramConsumer.RetrievePostsAsync(instagramRequest);

        var lastShared = lastState.LastShared.GetValueOrDefault(Platform);
        var postToShare = FindOldestNotSharedPost(postsNewestFirst, lastShared?.Id);

        if (postToShare is null)
        {
            Console.WriteLine($"No new posts to process. Exiting <{nameof(InstaWatcher)}.{nameof(Share)}>.");
            return;
        }

        Console.WriteLine($"New post detected: {postToShare.Id}");
        var postUrl = postToShare.Permalink ?? string.Empty;

        var isPublished = false;

        foreach (var publisher in socialPublishers)
        {
            var isPublishedByPublisher = await publisher.Publish(postUrl, environmentVars, Platform);

            if (isPublishedByPublisher)
            {
                isPublished = true;
            }
        }

        if (!isPublished)
        {
            Console.WriteLine($"No publisher shared {postToShare.Id}. State not saved.");
            return;
        }

        lastState.LastShared[SocialMedia.Instagram] = new(
            postToShare.Id,
            postUrl,
            postToShare.Caption ?? string.Empty,
            DateTime.Now);

        await gistConsumer.SaveCurrentState(lastState, environmentVars.GistOptions);
    }

    private static InstagramPost? FindOldestNotSharedPost(
        List<InstagramPost> postsNewestFirst,
        string? lastSharedPostId)
    {
        if (postsNewestFirst.Count == 0)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(lastSharedPostId))
        {
            return postsNewestFirst[^1];
        }

        var lastSharedIndex = postsNewestFirst.FindIndex(post => post.Id == lastSharedPostId);

        if (lastSharedIndex < 0)
        {
            Console.WriteLine($"[WARNING] Last shared post '{lastSharedPostId}' not found in the latest {PostsToFetch} posts. Halting to prevent reposts.");
            return null;
        }

        var isNewestAlreadyShared = lastSharedIndex == 0;

        if (isNewestAlreadyShared)
        {
            return null;
        }

        return postsNewestFirst[lastSharedIndex - 1];
    }
}
