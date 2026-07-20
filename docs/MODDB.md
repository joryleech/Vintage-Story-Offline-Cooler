# Vintage Story ModDB Listing

Published page: https://mods.vintagestory.at/show/mod/60475#tab-description

## Name

Offline Cooler

## Short description

A fat-sealed clay storage vessel that stops food spoilage while its owner is offline.

## Description

Offline Cooler adds a multiplayer-focused clay storage vessel crafted by
combining any fired storage vessel with one rendered fat.

The vessel records the UID and display name of the player who places it. While
that owner is offline, food inside does not spoil. Spoilage resumes when the
owner reconnects. The timer remains correct across unloaded chunks, repeated
login sessions, and server restarts.

The vessel retains the vanilla storage vessel's 12 slots and grain/vegetable
preservation bonuses. Its owner is visible in the placed-block information and
in the held-item tooltip. Breaking it preserves the old owner label for easy
identification; placing it again assigns it to the new placer.

### Crafting

The shapeless recipe uses:

- 1 fired clay storage vessel (any standard color)
- 1 rendered fat

### Requirements

- Vintage Story 1.22.0+
- Required on both server and client
- No third-party mod dependencies

## Version 1.0.0 changelog

- Added fat-sealed storage vessels in all standard clay colors.
- Added offline-owner spoilage prevention.
- Added persistent player UID and name ownership.
- Added owner labels to placed and held vessels.
- Added support for chunk unloads and server restarts.

## Suggested tags

Storage, Food, Multiplayer, Server, Survival

## Upload file

`offlinecooler-1.0.0.zip`

Use `docs/images/offlinecooler-icon.png` for the listing artwork and
`OfflineCooler/modicon.png` for the in-game mod icon.

## Release checklist

- Confirm `modinfo.json` and the DLL are at the root of the zip.
- Confirm the version in the filename and `modinfo.json` match.
- Upload `modicon.png` as the listing icon if the site requests it separately.
- Paste the version changelog into the release entry.
- Mark compatibility as Vintage Story 1.22.0 and newer.
