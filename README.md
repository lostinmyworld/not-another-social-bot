# not-another-social-bot

[![Not Another Social Bot](https://github.com/lostinmyworld/not-another-social-bot/actions/workflows/not-another-social-bot.yml/badge.svg?branch=main)](https://github.com/lostinmyworld/not-another-social-bot/actions/workflows/not-another-social-bot.yml)
[![Refresh Instagram Token](https://github.com/lostinmyworld/not-another-social-bot/actions/workflows/refresh-instagram-token.yml/badge.svg?branch=main)](https://github.com/lostinmyworld/not-another-social-bot/actions/workflows/refresh-instagram-token.yml)

A small .NET bot that watches your YouTube channel and Instagram account and shares each new post to Discord and Threads (and X).
It runs on a GitHub Actions schedule, so there is no server to host.

## What it does

- **Watches** YouTube and Instagram every hour.
- **Shares one post per run**, oldest not-shared first, so a backlog goes out gradually instead of all at once.
- **Remembers what it shared** in a GitHub Gist, so nothing is posted twice.
- **Fails loudly**: a broken token, API error, or a missing setting for an enabled watcher turns the run red instead of silently doing nothing.
- **Renews the Instagram and Threads tokens** automatically, twice a month.

## Sources and targets

| Watcher (source) | Status |
|---|---|
| YouTube | ✅ All videos on the channel |
| Instagram | ✅ Latest 100 posts |

| Publisher (target) | Status |
|---|---|
| Discord | ✅ YouTube: "New YouTube Video" message. Instagram: random header |
| Threads | ✅ Text post: title (first line) + link with preview |
| X (Twitter) | ⚠️ Implemented, needs a paid X API plan |

## How it works

```
GitHub Actions (hourly)
  └─ load last shared posts from Gist
  └─ for each enabled watcher (YouTube, Instagram)
       └─ find the oldest post not shared yet
       └─ send it to every enabled publisher (Discord, Threads, X)
       └─ at least one succeeded → save it to Gist
```

- If the last shared post can't be found anymore (deleted), the watcher stops and logs a warning instead of reposting old posts.
- A disabled watcher logs `Skipping <platform>` and moves on.

## Setup

The bot serves any number of accounts. Each account = one GitHub **environment** with the same setting names.
Change one setting → edit just that secret or variable.

Repository level (Settings → Secrets and variables → Actions):

| Name | Type | Value |
|---|---|---|
| `ACCOUNTS` | Variable | Account names = environment names, e.g. `["MINE","MARIA"]` |
| `IG_TOKEN_REFRESH_PAT` | Secret | Fine-grained PAT, this repo only, **Secrets: Read and write** and **Environments: Read and write**. Lets the refresh workflow save renewed tokens |

Per account (Settings → Environments → `<NAME>`):

| Secrets | Variables |
|---|---|
| `GIST_ID` | `GIST_STATE_FILE_NAME` |
| `GIST_TOKEN` | `WATCHER_YOUTUBE_ENABLED` |
| `YOUTUBE_API_KEY` | `YOUTUBE_CHANNEL_ID` |
| `DISCORD_WEBHOOK_YOUTUBE` | `WATCHER_INSTAGRAM_ENABLED` |
| `IG_ACCESS_TOKEN` | `PUBLISHER_DISCORD_ENABLED` |
| `DISCORD_WEBHOOK_INSTAGRAM` | `PUBLISHER_THREADS_ENABLED` |
| `THREADS_ACCESS_TOKEN` | |

Leave out what the account doesn't use. Flags are `true` or `false`.

### Add an account

1. Settings → Environments → **New environment** → name it, e.g. `MARIA`.
2. Add its secrets and variables from the table above. Give it its own `GIST_STATE_FILE_NAME`.
3. Add `<NAME>` to the `ACCOUNTS` variable.
4. Optional: seed its Gist state (see below) to skip old posts.

### Threads token

1. In your Meta app, add the **Access the Threads API** use case with `threads_basic` and `threads_content_publish`.
2. Add the Threads profile as a tester and accept the invite from that Threads profile.
3. Generate a long-lived token and put it in `THREADS_ACCESS_TOKEN`, with `PUBLISHER_THREADS_ENABLED=true`.

Remove an account → drop its name from `ACCOUNTS`. Pause everything → set `ACCOUNTS` to empty.

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

1. Copy [`account.env.example`](account.env.example) to `NotAnotherSocialBot/.env.local` and fill it in.
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
- [packages.SocialModels](https://github.com/lostinmyworld/packages.SocialModels) — shared clients for YouTube, Instagram, Discord, Threads, X and Gist
