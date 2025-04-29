# Death Count

A simple mod that tracks and displays your total player deaths during gameplay in [Game Name].

## Features

- Displays a persistent death counter while in gameplay levels.
- Counter style matches the game's UI.
- Starts at 0 deaths each time you start the game/mod.
- Detects player deaths and respawns accurately.

## Installation

1. Install BepInEx for R.E.P.O.
2. Download the `Death_Count` package.
3. Extract the contents of the zip file into your game's `BepInEx/plugins` folder.
4. The `DeathCount.dll` file should be in a subfolder (e.g., `BepInEx/plugins/Death_Count/DeathCount.dll`).

## Configuration

(Optional)

You can adjust the position of the death counter by modifying the `xPos` and `yPos` values in the `OnGUI()` method of the `DeathCount.cs` file if you are compiling it yourself.

For debugging, press F7 during gameplay to see scene information and debug state in the BepInEx console.
Press F8 to manually increment the death counter for testing.
Press F9 to manually decrement the death counter.
Press F10 to toggle force UI display mode (shows the counter everywhere).

## Changelog

See [CHANGELOG.md](CHANGELOG.md)

## Credits

- Mod developed by Ara (Your Name/Alias)
- Powered by BepInEx

## Support

If you encounter any issues or have suggestions, please report them [link to your GitHub issues page or other support channel, or remove if none].
