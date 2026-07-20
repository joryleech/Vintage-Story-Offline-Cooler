# Offline Cooler

<p align="center">
  <img src="docs/images/offlinecooler-icon.png" alt="Offline Cooler icon" width="256">
</p>

<p align="center">
  <a href="https://mods.vintagestory.at/show/mod/60475#tab-description">Vintage Story ModDB</a>
  ·
  <a href="https://github.com/joryleech/Vintage-Story-Offline-Cooler/releases">GitHub Releases</a>
</p>

Offline Cooler is a multiplayer-friendly Vintage Story mod that adds a
fat-sealed clay storage vessel. Food inside stops spoiling whenever the player
who placed the vessel is offline.

## Features

- Crafted from any fired clay storage vessel and one rendered fat.
- Preserves the original vessel's clay color.
- Provides the same 12 storage slots and grain/vegetable bonuses as the vanilla
  storage vessel.
- Records the placing player's UID and display name.
- Shows the owner on both the placed vessel and its item tooltip.
- Stops perish transitions while the owner is offline.
- Correctly accounts for unloaded chunks, repeated logins, and server restarts.
- Reassigns ownership to the next player who places the vessel.

## Requirements

- Vintage Story 1.22.0 or newer.
- The mod must be installed on both the server and every connecting client.

## Installation

1. Download `offlinecooler-<version>.zip` from the latest release.
2. Place the zip directly in the Vintage Story `Mods` folder. Do not extract it.
3. Enable **Offline Cooler** in the Mod Manager and restart the game or server.

## Crafting

The recipe is shapeless:

- 1 fired clay storage vessel, in any standard clay color
- 1 rendered fat

## Ownership and spoilage

Placing the vessel assigns it to that player. The placed-block information
shows the owner's name and UID, while the held-item tooltip shows the owner's
name (or UID if no name is available).

When the owner disconnects, perish transitions inside their placed vessels are
paused. They resume when that owner reconnects. Other players may still access
the vessel; preservation is based solely on whether its owner is online.

Breaking a vessel keeps its previous owner label on the dropped item for
identification. Placing it again assigns it to the new placer.

## Building from source

The project requires the .NET 10 SDK and a local Vintage Story installation.
Set `VINTAGE_STORY` to the installation directory containing
`VintagestoryAPI.dll`, then run:

```powershell
./build.ps1
```

On Linux/macOS:

```bash
./build.sh
```

The upload-ready archive is written to `Releases/`.

## Compatibility

Offline Cooler stores its data in normal block-entity attributes plus a small
save-game record of player offline time. Removing the mod makes its custom
vessels unavailable, so back up a world before uninstalling any content mod.

## License

Offline Cooler is available under the [MIT License](LICENSE).

See [CHANGELOG.md](CHANGELOG.md) for release history and
[docs/MODDB.md](docs/MODDB.md) for ready-to-paste ModDB listing text.
