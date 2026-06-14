using NotAnotherSocialBot.Models;
using Social.Models.Enums;

namespace NotAnotherSocialBot.Abstractions;

public interface ISocialWatcher
{
    SocialMedia Platform { get; }

    bool IsWatcherEnabled(EnvironmentVariables environmentVars);

    Task Share(
        LastState lastState,
        EnvironmentVariables environmentVars);
}
