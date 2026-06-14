using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.Models;
using Social.Models.Enums;
using Social.Models.Twitter;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot;

public class TwitterPublisher(ITwitterSharer twitterSharer) : SocialPublisherBase
{
    public override SocialMedia Platform => SocialMedia.X;

    public override async Task<bool> Publish(
        string url,
        EnvironmentVariables environmentVars,
        SocialMedia sourceSocial)
    {
        if (environmentVars?.TwitterParams is null
            || string.IsNullOrWhiteSpace(environmentVars.TwitterParams.ConsumerKey)
            || string.IsNullOrWhiteSpace(environmentVars.TwitterParams.ConsumerSecret)
            || string.IsNullOrWhiteSpace(environmentVars.TwitterParams.AccessToken)
            || string.IsNullOrWhiteSpace(environmentVars.TwitterParams.AccessSecret))
        {
            return false;
        }

        if (!IsPublisherEnabled(environmentVars))
        {
            return false;
        }

        var twitterCredentials = new TwitterCredentials(
            environmentVars.TwitterParams.ConsumerKey,
            environmentVars.TwitterParams.ConsumerSecret,
            environmentVars.TwitterParams.AccessToken,
            environmentVars.TwitterParams.AccessSecret);
        var twitterPostRequest = new TwitterPostRequest(url);

        var response = await twitterSharer.SharePost(
            twitterCredentials,
            twitterPostRequest);

        return response?.IsSuccess
            ?? false;
    }
}
