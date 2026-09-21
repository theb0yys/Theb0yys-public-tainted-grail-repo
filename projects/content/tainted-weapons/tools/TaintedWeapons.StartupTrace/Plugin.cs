using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaintedWeapons.StartupTrace;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("kane.tgfoa.tainted-weapons", BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "kane.tgfoa.tainted-weapons-startup-trace";
    public const string PluginName = "Tainted Weapons Startup Trace";
    public const string PluginVersion = "0.1.0";

    private const string SchemaVersion = "tainted-weapons-startup-trace-v1";
    private static readonly object WriteLock = new();
    private static Plugin? Current;

    private readonly List<string> _patchedMethods = new();
    private Harmony? _harmony;
    private string _outputPath = string.Empty;
    private string _taintedWeaponsSha256 = string.Empty;
    private string _taintedWeaponsMvid = string.Empty;
    private string _taintedWeaponsVersion = string.Empty;

    private void Awake()
    {
        Current = this;
        string receiptRoot = Path.Combine(Paths.BepInExRootPath, "plugins", "TaintedWeapons", "receipts");
        Directory.CreateDirectory(receiptRoot);
        _outputPath = Path.Combine(receiptRoot, "startup-trace-" + DateTimeOffset.UtcNow.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture) + ".jsonl");

        CaptureTaintedWeaponsAssemblyIdentity();
        Record("trace-plugin-awake", "event", string.Empty, "Non-mutating observer loaded. No registry, item, inventory, save, Babel, or Drake mutation is performed by this plugin.");

        _harmony = new Harmony(PluginGuid);
        ApplyTracePatches(_harmony);
        Record("trace-patches-applied", "event", string.Empty, "patched=" + string.Join(",", _patchedMethods));
        Logger.LogInfo($"{PluginName} {PluginVersion} active. output={_outputPath}; patched={_patchedMethods.Count.ToString(CultureInfo.InvariantCulture)}; mutation=false");
    }

    private void OnDestroy()
    {
        Record("trace-plugin-destroy", "event", string.Empty, "Observer shutting down.");
        _harmony?.UnpatchSelf();
        _harmony = null;
        if (ReferenceEquals(Current, this))
        {
            Current = null;
        }
    }

    private void ApplyTracePatches(Harmony harmony)
    {
        Patch(harmony, "Awaken.TG.Main.Scenes.SceneConstructors.ApplicationScene", "InitAll");
        Patch(harmony, "Awaken.TG.Main.Scenes.SceneConstructors.ApplicationScene", "InitLocalization");
        Patch(harmony, "Awaken.TG.Main.Scenes.SceneConstructors.ApplicationScene", "InitializeServices");

        Patch(harmony, "Awaken.Babel.BabelManager", "Initialize");
        Patch(harmony, "Awaken.Babel.BabelManager", "SwitchLanguage");
        Patch(harmony, "Awaken.Babel.BabelManager", "Dispose");

        Patch(harmony, "Awaken.TG.Main.Templates.TemplatesLoader", "CreateAndLoad");
        Patch(harmony, "Awaken.TG.Main.Templates.TemplatesLoader", "LoadAssetsInBuild");
        Patch(harmony, "Awaken.TG.Main.Templates.TemplatesLoader", "set_FinishedLoading");

        Patch(harmony, "Awaken.TG.Main.UI.TitleScreen.TitleScreenUI", "Awake");
        Patch(harmony, "Awaken.TG.Main.UI.TitleScreen.TitleScreenUI", "Start");
        Patch(harmony, "Awaken.TG.Main.UI.TitleScreen.TitleScreenUI", "OnMount");
    }

    private void Patch(Harmony harmony, string typeName, string methodName)
    {
        Type? type = AccessTools.TypeByName(typeName);
        if (type == null)
        {
            Record("patch-missing-type", "patch", typeName + "." + methodName, "Type was not found.");
            return;
        }

        MethodInfo[] methods = type
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(candidate => candidate.Name == methodName && !candidate.ContainsGenericParameters && !candidate.IsAbstract)
            .ToArray();
        if (methods.Length == 0)
        {
            Record("patch-missing-method", "patch", typeName + "." + methodName, "Method was not found.");
            return;
        }

        MethodInfo prefix = AccessTools.Method(typeof(Plugin), nameof(TracePrefix));
        MethodInfo postfix = AccessTools.Method(typeof(Plugin), nameof(TracePostfix));
        foreach (MethodInfo method in methods)
        {
            try
            {
                harmony.Patch(method, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));
                string identity = MethodIdentity(method);
                _patchedMethods.Add(identity);
                Record("patch-installed", "patch", identity, string.Empty);
            }
            catch (Exception ex)
            {
                Record("patch-failed", "patch", MethodIdentity(method), ex.GetType().Name + ":" + ex.Message);
            }
        }
    }

    private static void TracePrefix(MethodBase __originalMethod, object? __instance)
    {
        Current?.Record("method-enter", "prefix", MethodIdentity(__originalMethod), InstanceDetail(__instance));
    }

    private static void TracePostfix(MethodBase __originalMethod, object? __instance)
    {
        Current?.Record("method-exit", "postfix", MethodIdentity(__originalMethod), InstanceDetail(__instance));
    }

    private void CaptureTaintedWeaponsAssemblyIdentity()
    {
        Assembly? assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(candidate => candidate.GetName().Name == "TaintedWeapons");
        if (assembly == null)
        {
            return;
        }

        _taintedWeaponsVersion = assembly.GetName().Version?.ToString() ?? string.Empty;
        _taintedWeaponsMvid = assembly.ManifestModule.ModuleVersionId.ToString().ToUpperInvariant();
        try
        {
            if (!string.IsNullOrWhiteSpace(assembly.Location) && File.Exists(assembly.Location))
            {
                using var stream = File.Open(assembly.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using SHA256 sha = SHA256.Create();
                _taintedWeaponsSha256 = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Unable to fingerprint loaded TaintedWeapons assembly: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void Record(string eventName, string phase, string method, string detail)
    {
        try
        {
            Snapshot snapshot = CaptureSnapshot();
            var row = new StringBuilder(1024);
            row.Append('{');
            Add(row, "schemaVersion", SchemaVersion, first: true);
            Add(row, "capturedUtc", DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            Add(row, "event", eventName);
            Add(row, "phase", phase);
            Add(row, "method", method);
            Add(row, "frame", Time.frameCount);
            Add(row, "realtimeSinceStartup", Time.realtimeSinceStartup.ToString("R", CultureInfo.InvariantCulture));
            Add(row, "managedThreadId", Environment.CurrentManagedThreadId);
            Add(row, "selectedLocale", snapshot.SelectedLocale);
            Add(row, "templatesProviderPresent", snapshot.ProviderPresent);
            Add(row, "templatesAllLoaded", snapshot.AllLoaded);
            Add(row, "guidMapCount", snapshot.GuidMapCount);
            Add(row, "typeMapCount", snapshot.TypeMapCount);
            Add(row, "customRegisteredCount", snapshot.CustomRegisteredCount);
            Add(row, "mutationViolation", snapshot.CustomRegisteredCount.GetValueOrDefault() > 0);
            Add(row, "taintedWeaponsVersion", _taintedWeaponsVersion);
            Add(row, "taintedWeaponsMvid", _taintedWeaponsMvid);
            Add(row, "taintedWeaponsSha256", _taintedWeaponsSha256);
            Add(row, "detail", detail);
            row.Append('}');

            lock (WriteLock)
            {
                File.AppendAllText(_outputPath, row + Environment.NewLine, new UTF8Encoding(false));
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Startup trace record failed. event={eventName}; error={ex.GetType().Name}: {ex.Message}");
        }
    }

    private static Snapshot CaptureSnapshot()
    {
        var result = new Snapshot { SelectedLocale = GetSelectedLocale() };
        object? provider = GetTemplatesProvider();
        if (provider != null)
        {
            result.ProviderPresent = true;
            try
            {
                result.AllLoaded = AccessTools.Property(provider.GetType(), "AllLoaded")?.GetValue(provider) as bool?;
            }
            catch { }

            try
            {
                object? loader = AccessTools.Field(provider.GetType(), "_loader")?.GetValue(provider);
                if (loader != null)
                {
                    result.GuidMapCount = GetCount(AccessTools.Field(loader.GetType(), "guidMap")?.GetValue(loader));
                    result.TypeMapCount = GetCount(AccessTools.Field(loader.GetType(), "typeMap")?.GetValue(loader));
                }
            }
            catch { }
        }

        result.CustomRegisteredCount = GetCustomRegisteredCount();
        return result;
    }

    private static object? GetTemplatesProvider()
    {
        try
        {
            Type? worldType = AccessTools.TypeByName("Awaken.TG.MVC.World");
            Type? providerType = AccessTools.TypeByName("Awaken.TG.Main.Templates.TemplatesProvider");
            object? services = worldType == null ? null : AccessTools.Property(worldType, "Services")?.GetValue(null);
            if (services == null || providerType == null)
            {
                return null;
            }

            MethodInfo? tryGet = services.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(method => method.Name == "TryGet" && method.IsGenericMethodDefinition && method.GetParameters().Length == 1);
            if (tryGet != null)
            {
                object?[] args = { null };
                if (tryGet.MakeGenericMethod(providerType).Invoke(services, args) is bool success && success)
                {
                    return args[0];
                }
            }

            MethodInfo? get = services.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(method => method.Name == "Get" && method.IsGenericMethodDefinition && method.GetParameters().Length == 0);
            return get?.MakeGenericMethod(providerType).Invoke(services, Array.Empty<object>());
        }
        catch
        {
            return null;
        }
    }

    private static int? GetCustomRegisteredCount()
    {
        try
        {
            Type? pluginType = AccessTools.TypeByName("TaintedWeapons.Plugin");
            object? plugin = pluginType?.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
            object? status = pluginType?.GetProperty("NativeItemRegistrarStatus", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(plugin);
            return status?.GetType().GetProperty("RegisteredCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(status) is int count
                ? count
                : 0;
        }
        catch
        {
            return null;
        }
    }

    private static string GetSelectedLocale()
    {
        try
        {
            Type? settingsType = AccessTools.TypeByName("UnityEngine.Localization.Settings.LocalizationSettings");
            object? locale = settingsType == null ? null : AccessTools.Property(settingsType, "SelectedLocale")?.GetValue(null);
            object? identifier = locale == null ? null : AccessTools.Property(locale.GetType(), "Identifier")?.GetValue(locale);
            object? code = identifier == null ? null : AccessTools.Property(identifier.GetType(), "Code")?.GetValue(identifier);
            return code?.ToString() ?? identifier?.ToString() ?? locale?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static int? GetCount(object? value)
    {
        try
        {
            return value?.GetType().GetProperty("Count", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(value) is int count
                ? count
                : null;
        }
        catch
        {
            return null;
        }
    }

    private static string MethodIdentity(MethodBase method)
    {
        string parameters = string.Join(",", method.GetParameters().Select(parameter => parameter.ParameterType.FullName ?? parameter.ParameterType.Name));
        return (method.DeclaringType?.FullName ?? "<unknown>") + "." + method.Name + "(" + parameters + ")";
    }

    private static string InstanceDetail(object? instance)
    {
        if (instance == null) return "instance=null";
        if (instance is Object unityObject)
        {
            return "instanceType=" + instance.GetType().FullName + "; unityInstanceId=" + unityObject.GetInstanceID().ToString(CultureInfo.InvariantCulture);
        }
        return "instanceType=" + instance.GetType().FullName + "; objectId=" + RuntimeHelpers.GetHashCode(instance).ToString(CultureInfo.InvariantCulture);
    }

    private static void Add(StringBuilder builder, string name, string value, bool first = false)
    {
        if (!first) builder.Append(',');
        builder.Append('"').Append(Escape(name)).Append("\":\"").Append(Escape(value ?? string.Empty)).Append('"');
    }

    private static void Add(StringBuilder builder, string name, int value)
    {
        builder.Append(',').Append('"').Append(Escape(name)).Append("\":").Append(value.ToString(CultureInfo.InvariantCulture));
    }

    private static void Add(StringBuilder builder, string name, bool value)
    {
        builder.Append(',').Append('"').Append(Escape(name)).Append("\":").Append(value ? "true" : "false");
    }

    private static void Add(StringBuilder builder, string name, bool? value)
    {
        builder.Append(',').Append('"').Append(Escape(name)).Append("\":").Append(value.HasValue ? (value.Value ? "true" : "false") : "null");
    }

    private static void Add(StringBuilder builder, string name, int? value)
    {
        builder.Append(',').Append('"').Append(Escape(name)).Append("\":").Append(value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "null");
    }

    private static string Escape(string value)
    {
        var builder = new StringBuilder(value.Length + 16);
        foreach (char character in value)
        {
            switch (character)
            {
                case '\\': builder.Append("\\\\"); break;
                case '"': builder.Append("\\\""); break;
                case '\r': builder.Append("\\r"); break;
                case '\n': builder.Append("\\n"); break;
                case '\t': builder.Append("\\t"); break;
                default:
                    if (character < 0x20)
                    {
                        builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(character);
                    }
                    break;
            }
        }
        return builder.ToString();
    }

    private sealed class Snapshot
    {
        public string SelectedLocale { get; set; } = string.Empty;
        public bool ProviderPresent { get; set; }
        public bool? AllLoaded { get; set; }
        public int? GuidMapCount { get; set; }
        public int? TypeMapCount { get; set; }
        public int? CustomRegisteredCount { get; set; }
    }
}
