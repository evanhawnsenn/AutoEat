# Install AutoEat

This guide is for **Space Engineers 1** players using the **Pulsar** plugin loader on Windows.

## Recommended option — after PluginHub approval

Once the AutoEat proposal is approved on PluginHub:

1. Open Pulsar.
2. Refresh the plugin list, if that option is available.
3. Search for **AutoEat**.
4. Enable it in the profile you use to play.
5. Start Space Engineers through Pulsar.

AutoEat appears in the character Terminal, on the **Control Panel** tab. Use `Ctrl + Alt + U` to quickly enable or disable automatic eating.

## Manual installation — while approval is pending

1. Open the [latest release](https://github.com/evanhawnsenn/AutoEat/releases/latest).
2. Under **Assets**, download **Source code (zip)**.
3. Extract the ZIP to a permanent folder, for example `C:\Games\PulsarPlugins\AutoEat`.
   - Do not extract it to `Downloads` if you regularly clean that folder.
4. Close Space Engineers and open Pulsar with the `-sources` parameter:
   - if you use a shortcut, right-click it, open **Properties**, and add ` -sources` to the end of the **Target** field;
   - for example: `C:\Path\To\Pulsar\Interim.exe -sources`.
5. In the Pulsar window, click **Sources**.
6. Choose the option to add a **Development Folder** / **Local Plugin Source**, then select the folder extracted in step 3.
   - select the folder containing `Plugin.cs`, `AutoEatRuntime.cs`, and `Settings.cs`;
   - do not select the parent folder or the ZIP file.
7. Return to Pulsar's plugin list, enable **AutoEat** in the current profile, then save/apply the change.
8. Start Space Engineers through Pulsar.

> The `-sources` mode is only needed to register the local folder. After that, you can open Pulsar normally.

### If the Sources button does not appear

Confirm that Pulsar was started with `-sources`. Close it completely, then open it again with the modified shortcut. If you start the game through Steam, open Pulsar once with that parameter before returning to your normal launch method.

### How to confirm it loaded

- Enter a world and open your character's Terminal.
- Go to **Control Panel**.
- The **AUTOEAT** section must appear only in the character Terminal — not in the terminals of base blocks.

## Initial configuration

- **Automatic eating:** enables or disables the feature.
- **Eat below:** hunger percentage at which the character tries to eat.
- **Warn when no food:** enables the alert when no food is available in the personal inventory.
- **Warn below:** hunger percentage that triggers the alert.
- **Food priority:** sets the order in which food items are used.

The first alert appears immediately when the configured limit is reached, then repeats every 30 seconds while no food is available.

## If it does not work

1. Confirm that the game was started through Pulsar, rather than directly through Steam.
2. Confirm that AutoEat is enabled in the correct Pulsar profile.
3. Close the game, restart Pulsar, and try again.
4. Do not enable the old world mod named AutoEat at the same time.
5. Open Pulsar's debug panel and send the error screenshot to the author.

## Notes

- The plugin looks for compatible food in the character's personal inventory.
- The interface uses Portuguese when the game is in Portuguese and English for all other languages.
- AutoEat is an independent project created by Denis Martins with assistance from OpenAI Codex (AI).
