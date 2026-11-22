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

The bot serves any number of accounts. Each account = one secret with all its settings.
(Settings → Secrets and variables → Actions)

| Name | Type | Value |
|---|---|---|
| `ACCOUNTS` | Variable | Account names, e.g. `["MINE","MARIA"]`. Letters, digits, `_` only |
| `ACCOUNT_<NAME>` | Secret, one per account | Contents of [`account.env.example`](account.env.example), filled in |
| `IG_TOKEN_REFRESH_PAT` | Secret, shared | Fine-grained PAT, this repo only, **Secrets: Read and write**. Lets the refresh workflow save renewed tokens |

### Add an account

1. Copy [`account.env.example`](account.env.example) and fill in what the account uses (YouTube, Instagram or both).
   Give it its own `GIST_STATE_FILE_NAME`.
2. Add it as secret `ACCOUNT_<NAME>`, e.g. `ACCOUNT_MARIA`.
3. Add `<NAME>` to the `ACCOUNTS` variable.
4. Optional: seed its Gist state (see below) to skip old posts.

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

1. Create `NotAnotherSocialBot/.env.local` with the same content as an `ACCOUNT_<NAME>` secret.
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
