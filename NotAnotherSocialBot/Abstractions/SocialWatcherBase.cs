using NotAnotherSocialBot.Models;
using Social.Models.Enums;

namespace NotAnotherSocialBot.Abstractions;

public abstract class SocialWatcherBase : ISocialWatcher
{
    public virtual SocialMedia Platform => throw new NotImplementedException();

    public bool IsWatcherEnabled(EnvironmentVariables environmentVars)
        => environmentVars!.EnabledWatchers.TryGetValue(Platform, out var watcherEnabled)
            && watcherEnabled;

    public abstract Task Share(LastState lastState, EnvironmentVariables environmentVars);
}
