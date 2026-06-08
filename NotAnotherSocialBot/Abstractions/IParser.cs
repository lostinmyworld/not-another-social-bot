using NotAnotherSocialBot.Models;

namespace NotAnotherSocialBot.Abstractions;

public interface IParser
{
    EnvironmentVariables GetEnvironmentVariables();
}
