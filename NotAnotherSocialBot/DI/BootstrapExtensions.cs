using Microsoft.Extensions.DependencyInjection;
using NotAnotherSocialBot.Abstractions;
using Social.Oversharers.Extensions;
using Social.Overthinkers.Extensions;

namespace NotAnotherSocialBot.DI;

public static class BootstrapExtensions
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSocialOverThinkers();
        services.AddSocialOverSharers();

        services.AddSingleton<IParser, Parser>();

        services.AddSingleton<ISocialWatcher, YouTubeWatcher>();
        services.AddSingleton<ISocialWatcher, InstaWatcher>();

        services.AddSingleton<ISocialPublisher, DiscordPublisher>();
        services.AddSingleton<ISocialPublisher, TwitterPublisher>();

        return services;
    }
}
