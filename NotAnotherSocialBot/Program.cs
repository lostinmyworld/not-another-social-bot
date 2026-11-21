using Microsoft.Extensions.DependencyInjection;
using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.DI;
using NotAnotherSocialBot.Models;
using Social.Oversharers.Abstractions;

var services = new ServiceCollection();

services.AddDependencies();

var serviceProvider = services.BuildServiceProvider();

var parser = serviceProvider.GetRequiredService<IParser>();
var gistConsumer = serviceProvider.GetRequiredService<IGistConsumer>();

var environmentVars = parser.GetEnvironmentVariables();

var lastState = await gistConsumer.LoadPreviousState<LastState>(environmentVars.GistOptions, "NotAnotherSocialBot");

Console.WriteLine("Polling YouTube...");

var watchers = serviceProvider.GetServices<ISocialWatcher>();

foreach (var watcher in watchers)
{
    Console.WriteLine($"Starting watcher for {watcher.Platform}...");

    try
    {
        await watcher.Share(lastState, environmentVars);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Watcher for {watcher.Platform} failed with message: {ex.Message}.");
        Environment.ExitCode = 1;
    }
}