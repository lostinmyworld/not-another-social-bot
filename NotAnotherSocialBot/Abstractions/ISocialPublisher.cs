using NotAnotherSocialBot.Models;
using Social.Models.Enums;

namespace NotAnotherSocialBot.Abstractions;

public interface ISocialPublisher
{
    SocialMedia Platform { get; }

    bool IsPublisherEnabled(EnvironmentVariables environmentVars);

    Task<bool> Publish(
        PostToShare post,
        EnvironmentVariables environmentVars,
        SocialMedia sourceSocial);
}
