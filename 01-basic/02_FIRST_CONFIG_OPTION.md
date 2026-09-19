# 02 - Add Your First Config Option

Configuration lets a user turn a feature on/off without rebuilding the mod.

## IL2CPP

Use this pattern in your IL2CPP Plugin.cs:

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

Build and redeploy using the same command from level 00.

Launch the game once.

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

Your log should now report Enabled=False.

## Mono

The supplied Mono starter already demonstrates a Config.Bind(...) boolean.

Use the same experiment:

1. launch once;
2. find the generated config;
3. change the value;
4. relaunch;
5. confirm the log reports the changed value.

## Rule

A config value should be:

- named clearly;
- documented;
- safe by default;
- bounded if it is numeric or free-form input.

Do not store passwords/tokens in ordinary mod config.
