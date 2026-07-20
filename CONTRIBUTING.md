# Contributing

Bug reports and focused pull requests are welcome.

## Reporting a bug

Include:

- Vintage Story version
- Offline Cooler version
- Single-player or multiplayer/dedicated-server environment
- Reproduction steps
- Relevant client and server log excerpts
- Other installed mods that may affect inventories or food transitions

## Development

1. Install the .NET 10 SDK and Vintage Story 1.22 or newer.
2. Set `VINTAGE_STORY` to the game installation directory.
3. Build with `./build.ps1` on Windows or `./build.sh` elsewhere.
4. Verify the generated zip loads on both a client and a dedicated server.

Keep changes scoped, preserve existing save compatibility when possible, and
update `CHANGELOG.md` for player-visible behavior changes.
