using Social.Models;
using Social.Models.Enums;

namespace NotAnotherSocialBot.Abstractions;

public interface ISocialPublisher
{
    SocialMedia Platform { get; }

    Task PublishAsync(SocialPost post);
}
