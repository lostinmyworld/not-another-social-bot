using Social.Models.Enums;

namespace NotAnotherSocialBot.Models;

public class LastState
{
    public Dictionary<SocialMedia, LastPost> LastShared { get; set; } = [];
}