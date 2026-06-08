using Microsoft.Extensions.DependencyInjection;
using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.DI;
using NotAnotherSocialBot.Models;
using Social.Models.Discord;
using Social.Models.Enums;
using Social.Oversharers.Abstractions;

var services = new ServiceCollection();

services.AddDependencies();

var serviceProvider = services.BuildServiceProvider();

var parser = serviceProvider.GetRequiredService<IParser>();
var gistConsumer = serviceProvider.GetRequiredService<IGistConsumer>();
var discordSharer = serviceProvider.GetRequiredService<IDiscordSharer>();

var environmentVars = parser.GetEnvironmentVariables();
var youTubeToDiscordWebHook = Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_YOUTUBE")!;

var lastState = await gistConsumer.LoadPreviousState<LastState>(environmentVars.GistOptions, "NotAnotherSocialBot");

Console.WriteLine("Polling YouTube...");

var youtubeConsumer = serviceProvider.GetRequiredService<IYouTubeConsumer>();
var oldestYouTubeVideo = await youtubeConsumer.RetrieveOldestNotSharedVideo(
    environmentVars.YouTubeChannelId,
    lastState.LastProcessedIds[SocialMedia.YouTube]);

if (oldestYouTubeVideo is null
    || string.IsNullOrWhiteSpace(oldestYouTubeVideo.VideoId)
    || oldestYouTubeVideo.VideoId == lastState.LastProcessedIds[SocialMedia.YouTube])
{
    Console.WriteLine("No new videos to process. Exiting.");
    return;
}

Console.WriteLine($"New video detected: {oldestYouTubeVideo.Title} ({oldestYouTubeVideo.VideoId})");
var videoUrl = $"https://www.youtube.com/watch?v={oldestYouTubeVideo.VideoId}";

var discordRequest = new DiscordRequest
{
    Content = $"**New Video @everyone !**\n\n{videoUrl}",
};

await discordSharer.SendToDiscord(discordRequest, youTubeToDiscordWebHook);

lastState.LastProcessedIds[SocialMedia.YouTube] = oldestYouTubeVideo.VideoId;

await gistConsumer.SaveCurrentState(lastState, environmentVars.GistOptions);
