using Social.Models.Enums;

namespace NotAnotherSocialBot.Models;

public class LastState
{
    public Dictionary<SocialMedia, string> LastProcessedIds { get; set; } = [];
}