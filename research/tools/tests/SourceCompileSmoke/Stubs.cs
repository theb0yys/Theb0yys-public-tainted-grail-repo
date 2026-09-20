using System;
using System.Collections.Generic;
using System.Reflection;

namespace BepInEx
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BepInPlugin : Attribute
    {
        public BepInPlugin(string guid, string name, string version)
        {
        }
    }

    public abstract class BaseUnityPlugin
    {
        public Configuration.ConfigFile Config { get; } = new Configuration.ConfigFile();
        public Logging.ManualLogSource Logger { get; } = new Logging.ManualLogSource();
    }
}

namespace BepInEx.Configuration
{
    public sealed class ConfigEntry<T>
    {
        public ConfigEntry(T value)
        {
            Value = value;
        }

        public T Value { get; set; }
    }

    public sealed class ConfigFile
    {
        public ConfigEntry<T> Bind<T>(string section, string key, T value, string description)
        {
            return new ConfigEntry<T>(value);
        }

        public ConfigEntry<T> Bind<T>(string section, string key, T value, ConfigDescription description)
        {
            return new ConfigEntry<T>(value);
        }
    }

    public sealed class ConfigDescription
    {
        public ConfigDescription(string description, object? acceptableValues = null, params object[] tags)
        {
        }
    }

    public sealed class AcceptableValueRange<T>
    {
        public AcceptableValueRange(T minValue, T maxValue)
        {
        }
    }
}

namespace BepInEx.Logging
{
    public sealed class ManualLogSource
    {
        public void LogInfo(object data)
        {
        }

        public void LogWarning(object data)
        {
        }

        public void LogError(object data)
        {
        }
    }
}

namespace HarmonyLib
{
    public enum HarmonyPatchType
    {
        All,
        Prefix,
        Postfix,
        Transpiler,
        Finalizer
    }

    public sealed class Harmony
    {
        public Harmony(string id)
        {
        }

        public MethodInfo? Patch(
            MethodBase original,
            HarmonyMethod? prefix = null,
            HarmonyMethod? postfix = null,
            HarmonyMethod? transpiler = null,
            HarmonyMethod? finalizer = null)
        {
            return null;
        }

        public void UnpatchSelf()
        {
        }

        public void Unpatch(MethodBase original, HarmonyPatchType type, string harmonyID = "*")
        {
        }

        public static Patches? GetPatchInfo(MethodBase method)
        {
            return new Patches();
        }

        public static IEnumerable<MethodBase> GetAllPatchedMethods()
        {
            return Array.Empty<MethodBase>();
        }
    }

    public sealed class HarmonyMethod
    {
        public HarmonyMethod(MethodInfo method)
        {
        }
    }

    public sealed class Patches
    {
        public Patch[] Prefixes { get; set; } = Array.Empty<Patch>();
        public Patch[] Postfixes { get; set; } = Array.Empty<Patch>();
        public Patch[] Transpilers { get; set; } = Array.Empty<Patch>();
        public Patch[] Finalizers { get; set; } = Array.Empty<Patch>();
    }

    public sealed class Patch
    {
        public string owner = string.Empty;
    }

    public static class AccessTools
    {
        public static MethodInfo? Method(Type type, string name)
        {
            return typeof(object).GetMethod(nameof(object.ToString));
        }

        public static MethodInfo? Method(Type type, string name, Type[] parameters)
        {
            return typeof(object).GetMethod(nameof(object.ToString));
        }
    }
}

namespace UnityEngine
{
    public static class Application
    {
        public static string version => "test";
        public static string unityVersion => "test";
    }

    public sealed class WaitForSecondsRealtime
    {
        public WaitForSecondsRealtime(float seconds)
        {
        }
    }
}
