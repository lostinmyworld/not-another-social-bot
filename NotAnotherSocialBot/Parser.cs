using NotAnotherSocialBot.Abstractions;
using NotAnotherSocialBot.Models;
using Social.Models.Gist;
using Social.Oversharers.Abstractions;

namespace NotAnotherSocialBot;

public class Parser : IParser
{
    private readonly EnvironmentVariables _environmentVars;

    public Parser(IEnvironmentLoader environmentLoader)
    {
        var gistOptionsToRetrieve = new GistOptions()
        {
            GistId = "GIST_ID",
            GistToken = "GIST_TOKEN",
            GistStateFileName = "GIST_STATE_FILE_NAME",
        };
        var gistOptions = environmentLoader.LoadGistOptions(gistOptionsToRetrieve)!;

        _environmentVars = RetrieveEnvironmentVariables(gistOptions);
    }

    public EnvironmentVariables GetEnvironmentVariables()
    {
        return _environmentVars;
    }

    private static EnvironmentVariables RetrieveEnvironmentVariables(GistOptions gistOptions)
    {
        var youTubeChannelId = Environment.GetEnvironmentVariable("YOUTUBE_CHANNEL_ID")!;

        return new(gistOptions, youTubeChannelId);
    }
}
