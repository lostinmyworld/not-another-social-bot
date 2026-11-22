using NotAnotherSocialBot.Models;
using Social.Models.Enums;

namespace NotAnotherSocialBot.Abstractions;

public abstract class SocialPublisherBase : ISocialPublisher
{
    public virtual SocialMedia Platform => throw new NotImplementedException();

    public virtual bool IsPublisherEnabled(EnvironmentVariables environmentVars)
        => environmentVars!.EnabledPublishers.TryGetValue(Platform, out var publisherEnabled)
            && publisherEnabled;

    public abstract Task<bool> Publish(PostToShare post, EnvironmentVariables environmentVars, SocialMedia sourceSocial);
}
