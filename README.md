# not-another-social-bot

[![Not Another Social Bot](https://github.com/lostinmyworld/not-another-social-bot/actions/workflows/not-another-social-bot.yml/badge.svg?branch=main)](https://github.com/lostinmyworld/not-another-social-bot/actions/workflows/not-another-social-bot.yml)
[![Refresh Instagram Token](https://github.com/lostinmyworld/not-another-social-bot/actions/workflows/refresh-instagram-token.yml/badge.svg?branch=main)](https://github.com/lostinmyworld/not-another-social-bot/actions/workflows/refresh-instagram-token.yml)

A small .NET bot that watches your YouTube channel and Instagram account and shares each new post to Discord (and X).
It runs on a GitHub Actions schedule, so there is no server to host.

## What it does

- **Watches** YouTube and Instagram every hour.
- **Shares one post per run**, oldest not-shared first, so a backlog goes out gradually instead of all at once.
- **Remembers what it shared** in a GitHub Gist, so nothing is posted twice.
- **Fails loudly**: a broken token, API error, or a missing setting for an enabled watcher turns the run red instead of silently doing nothing.
- **Renews the Instagram token** automatically, twice a month.

## Sources and targets

| Watcher (source) | Status |
|---|---|
| YouTube | ✅ All videos on the channel |
| Instagram | ✅ Latest 100 posts |

| Publisher (target) | Status |
|---|---|
| Discord | ✅ YouTube: "New YouTube Video" message. Instagram: random header |
| X (Twitter) | ⚠️ Implemented, needs a paid X API plan |

## How it works

```
GitHub Actions (hourly)
  └─ load last shared posts from Gist
  └─ for each enabled watcher (YouTube, Instagram)
       └─ find the oldest post not shared yet
       └─ send it to every enabled publisher (Discord, X)
       └─ at least one succeeded → save it to Gist
```

- If the last shared post can't be found anymore (deleted), the watcher stops and logs a warning instead of reposting old posts.
- A disabled watcher logs `Skipping <platform>` and moves on.

## Setup

Add these as **repository secrets** (Settings → Secrets and variables → Actions).

| Secret | Used for |
|---|---|
| `GIST_ID`, `GIST_TOKEN`, `GIST_STATE_FILE_NAME` | Where the shared-posts state is stored. Token needs the `gist` scope |
| `WATCHER_YOUTUBE_ENABLED` | `true` to watch YouTube |
| `YOUTUBE_CHANNEL_ID`, `YOUTUBE_API_KEY` | YouTube Data API |
| `WATCHER_INSTAGRAM_ENABLED` | `true` to watch Instagram |
| `IG_ACCESS_TOKEN` | Long-lived Instagram token (Instagram API with Instagram Login) |
| `IG_TOKEN_REFRESH_PAT` | Fine-grained PAT, this repo only, **Secrets: Read and write**. Lets the refresh workflow save the renewed token |
| `PUBLISHER_DISCORD_ENABLED` | `true` to post to Discord |
| `DISCORD_WEBHOOK_YOUTUBE`, `DISCORD_WEBHOOK_INSTAGRAM` | Discord webhook per source |
| `PUBLISHER_TWITTER_ENABLED`, `WATCHER_TWITTER_ENABLED` | X support (paid API) |

Optional **repository variable**: `DISABLE_WATCHDOG_MINE` = `true` pauses the bot.

### Starting point

- **Empty state** → each watcher starts from its oldest post (Instagram: oldest of the latest 100) and moves forward one post per run.
- **Start from now** → put the latest post's ID in the Gist state before the first run:

```json
{
  "LastShared": {
    "YouTube": { "Id": "<video id>", "UrlToShare": "", "Title": "", "DetectedAt": "2026-01-01T00:00:00" },
    "Instagram": { "Id": "<post id>", "UrlToShare": "", "Title": "", "DetectedAt": "2026-01-01T00:00:00" }
  }
}
```

## Run locally

1. Create `NotAnotherSocialBot/.env.local` with the same keys as the secrets above (`KEY=value`, one per line).
   It is gitignored. **Never commit it.**
2. Run from the project folder, so `.env.local` is found:

```powershell
cd NotAnotherSocialBot
dotnet run
```

> A local run posts for real and updates the real Gist state.

## Tests

```powershell
dotnet test NotAnotherSocialBot.Tests
```

## Built with

- .NET 10
- [packages.SocialModels](https://github.com/lostinmyworld/packages.SocialModels) — shared clients for YouTube, Instagram, Discord, X and Gist
