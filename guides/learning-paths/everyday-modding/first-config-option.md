# Add Your First Config Option

## What you're doing

You are adding a user-editable setting so a feature can be enabled or disabled without rebuilding the mod.

## What you need

- a plug-in that already builds, deploys, and loads;
- the correct Mono or IL2CPP BepInEx lane;
- access to the generated BepInEx config directory.

## What you'll learn

You will learn how to:

- bind a BepInEx configuration value;
- find the generated config file;
- change a value outside the source code;
- verify that the plug-in reads the changed value on the next launch.

## Steps

### 1. Add a config value in IL2CPP

Use this pattern in your IL2CPP `Plugin.cs`:

~~~csharp
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;

namespace MyFirstTGMod;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "yourname.taintedgrail.myfirstmod";
    public const string PluginName = "My First Tainted Grail Mod";
    public const string PluginVersion = "0.2.0";

    private ConfigEntry<bool>? _enabled;

    public override void Load()
    {
        _enabled = Config.Bind(
            "General",
            "Enabled",
            true,
            "Enable this mod.");

        Log.LogInfo($"{PluginName} {PluginVersion} loaded. Enabled={_enabled.Value}");
    }
}
~~~

Build and redeploy using the same loop you already proved.

### 2. Launch once and find the generated config

BepInEx should create a config file using your plug-in GUID under:

~~~text
BepInEx\config\
~~~

Open the generated config and change:

~~~text
Enabled = true
~~~

to:

~~~text
Enabled = false
~~~

Launch again.

Your log should now report `Enabled=False`.

### 3. Repeat the same experiment on Mono

The supplied Mono starter already demonstrates a `Config.Bind(...)` boolean.

Use the same experiment:

1. launch once;
2. find the generated config;
3. change the value;
4. relaunch;
5. confirm the log reports the changed value.

### 4. Keep config values safe

A config value should be:

- named clearly;
- documented;
- safe by default;
- bounded if it is numeric or free-form input.

Do not store passwords or tokens in ordinary mod config.

## What success looks like

You can change the generated configuration value from `true` to `false`, relaunch without rebuilding, and see the plug-in report the new value.

## Common problems

**No config file appears:** first confirm that the plug-in actually loaded and reached the code that calls `Config.Bind`.

**You edited the wrong config file:** match the filename/config identity to your plug-in GUID.

**A changed value is ignored:** confirm you saved the file and restarted the game so the plug-in reads it again.

**A config field can accept unsafe or meaningless values:** add sensible defaults and bounds rather than trusting arbitrary input.

## Where to go next

Continue to **[Run the Harmony Self-Test](harmony-self-test.md)**.
