using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.Models;
using Social.Models.Enums;

namespace NotAnotherSocialBot;

public class InstaWatcher : SocialWatcherBase
{
    public override SocialMedia Platform => SocialMedia.Instagram;

    public override async Task Share(
        LastState lastState,
        EnvironmentVariables environmentVars)
    {
        if (!IsWatcherEnabled(environmentVars))
        {
            return;
        }

        throw new NotImplementedException();
    }
}
