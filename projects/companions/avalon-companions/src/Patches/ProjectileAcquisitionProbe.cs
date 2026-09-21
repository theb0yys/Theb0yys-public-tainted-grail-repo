using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Awaken.TG.Main.AI.Fights.Projectiles;
using Awaken.TG.Main.AI.Grid;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Combat;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Locations;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AvalonCompanions.Patches;

/// <summary>
/// Default-off, read-only observer for the exact Taming Bow + Taming Arrow
/// pair supplied by Tainted Weapons. Avalon Companions borrows the immutable
/// registered identities through the owning service and records only the
/// creature-acquisition observation. Ordinary bows and arrows are ignored.
/// </summary>
internal static class ProjectileAcquisitionProbe
{
    internal const string ReviewedWolfTemplateGuid = "9086dee514edc644b9b55890d885db3f";
    internal const string ReviewedWolfTemplateName = "Spec_AnimalWolf";

    private const string TaintedWeaponsPluginGuid = "kane.tgfoa.tainted-weapons";
    private const string TamingToolsApiTypeName = "TaintedWeapons.TaintedWeaponsTamingToolsApi";
    private const string TamingToolsEnsureMethodName = "EnsureRegistered";
    private const string CsvFileName = "acquisition-projectile-probe.csv";
    private const int MaxRowsPerGeneration = 500;
    private const float ToolRegistrationRetrySeconds = 1f;

    private const string CsvHeader =
        "utcTimestamp,sequence,sessionGeneration,scene,toolSetStatus,toolSetReason,exactToolPair," +
        "shooterIsCurrentHero,ownerRuntimeType,externalWeaponTemplateGuid,externalWeaponTemplateName," +
        "externalWeaponItemName,externalAmmunitionTemplateGuid,externalAmmunitionTemplateName," +
        "externalAmmunitionItemName,projectileRuntimeType,hitColliderName,hitColliderInstanceId," +
        "hitAliveRuntimeType,environmentHit,hitX,hitY,hitZ,targetLocationId,targetTemplateGuid," +
        "targetTemplateName,targetIsNpc,targetIsReviewedWolf,targetIsUnique,targetIsSummonOrAlly," +
        "targetIsAlive,targetIsDiscarded,targetIsUnconscious,boundWolfLocationId,boundOnThisObservation," +
        "locationMatchesBoundTarget,referenceMatchesBoundTarget,outcome,rejectionReason," +
        "mutation,ai,dialogue,quest,persistence";

    private static readonly object FileGate = new();

    private static ManualLogSource? _logger;
    private static ConfigEntry<bool>? _enabled;
    private static ConfigEntry<bool>? _writeCsv;
    private static ConfigEntry<bool>? _bindFirstReviewedWolfHit;

    private static bool _applied;
    private static bool _sceneEventSubscribed;
    private static bool _sessionInitialized;
    private static bool _rowLimitLogged;
    private static int _sessionGeneration;
    private static int _rowCount;
    private static long _observationSequence;
    private static string _sceneName = string.Empty;
    private static string _boundWolfLocationId = string.Empty;
    private static WeakReference<NpcElement>? _boundWolfActor;

    private static float _nextToolRegistrationAttemptAt;
    private static string _lastToolReceiptLog = string.Empty;
    private static string _toolSetStatus = "not-requested";
    private static string _toolSetReason = string.Empty;
    private static bool _toolSetRegistered;
    private static string _tamingBowTemplateGuid = string.Empty;
    private static string _tamingBowTemplateName = string.Empty;
    private static string _tamingArrowTemplateGuid = string.Empty;
    private static string _tamingArrowTemplateName = string.Empty;

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        if (_applied)
        {
            return;
        }

        _logger = logger;

        try
        {
            if (!Chainloader.PluginInfos.TryGetValue(Plugin.PluginGuid, out var pluginInfo)
                || pluginInfo.Instance is not Plugin plugin)
            {
                logger.LogWarning("Acquisition projectile probe was not registered because the Avalon Companions plugin instance was unavailable.");
                return;
            }

            _enabled = plugin.Config.Bind(
                "AcquisitionProbe",
                "Enabled",
                false,
                "Enable the default-off read-only Taming Bow/Taming Arrow projectile and wolf diagnostic. Ordinary bows and arrows are ignored. No gameplay state is changed.");
            _writeCsv = plugin.Config.Bind(
                "AcquisitionProbe",
                "WriteCsv",
                true,
                "Append bounded exact-pair projectile contact rows to acquisition-projectile-probe.csv while AcquisitionProbe.Enabled=true.");
            _bindFirstReviewedWolfHit = plugin.Config.Bind(
                "AcquisitionProbe",
                "BindFirstReviewedWolfHit",
                true,
                "Bind the first player-fired Taming Arrow hit on Spec_AnimalWolf in the current scene as the runtime-only diagnostic target.");

            MethodInfo? contactMethod = AccessTools.Method(
                typeof(Arrow),
                "OnContactEnding",
                new[] { typeof(HitResult), typeof(Collider), typeof(IAlive), typeof(bool) });
            MethodInfo? contactPrefix = AccessTools.Method(
                typeof(ProjectileAcquisitionProbe),
                nameof(BeforeArrowContactEnding));
            MethodInfo? pluginUpdateMethod = AccessTools.Method(typeof(Plugin), "Update");
            MethodInfo? updatePostfix = AccessTools.Method(
                typeof(ProjectileAcquisitionProbe),
                nameof(AfterPluginUpdate));
            MethodInfo? pluginDestroyMethod = AccessTools.Method(typeof(Plugin), "OnDestroy");
            MethodInfo? shutdownPrefix = AccessTools.Method(
                typeof(ProjectileAcquisitionProbe),
                nameof(BeforePluginDestroy));

            if (contactMethod == null
                || contactPrefix == null
                || pluginUpdateMethod == null
                || updatePostfix == null
                || pluginDestroyMethod == null
                || shutdownPrefix == null)
            {
                logger.LogWarning("Acquisition projectile probe failed closed because its native contact, update, or shutdown seam could not be resolved.");
                return;
            }

            harmony.Patch(pluginDestroyMethod, prefix: new HarmonyMethod(shutdownPrefix));
            harmony.Patch(pluginUpdateMethod, postfix: new HarmonyMethod(updatePostfix));
            harmony.Patch(contactMethod, prefix: new HarmonyMethod(contactPrefix));

            _applied = true;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            _sceneEventSubscribed = true;
            _enabled.SettingChanged += OnEnabledSettingChanged;

            logger.LogInfo(
                "Registered default-off exact-pair acquisition projectile probe. "
                + $"Enabled={_enabled.Value}; WriteCsv={_writeCsv.Value}; BindFirstReviewedWolfHit={_bindFirstReviewedWolfHit.Value}; "
                + "itemOwner=TaintedWeapons; ordinaryProjectilesIgnored=true; mutation=false; ai=false; dialogue=false; quest=false; persistence=false.");
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Acquisition projectile probe registration failed closed: {ex.GetType().Name}: {ex.Message}");
            Shutdown();
        }
    }

    private static void AfterPluginUpdate()
    {
        if (!IsEnabled || _toolSetRegistered || Time.realtimeSinceStartup < _nextToolRegistrationAttemptAt)
        {
            return;
        }

        _nextToolRegistrationAttemptAt = Time.realtimeSinceStartup + ToolRegistrationRetrySeconds;
        TryRefreshTamingToolSet();
    }

    private static void BeforeArrowContactEnding(
        Arrow __instance,
        HitResult __0,
        Collider? __1,
        IAlive? __2,
        bool __3)
    {
        if (!IsEnabled)
        {
            return;
        }

        try
        {
            if (!_toolSetRegistered && !TryRefreshTamingToolSet())
            {
                return;
            }

            Item? sourceWeapon = Safe(() => __instance.SourceWeapon);
            Item? sourceProjectile = Safe(() => __instance.SourceProjectile);
            ItemTemplate? weaponTemplate = Safe(() => sourceWeapon?.Template);
            ItemTemplate? ammunitionTemplate = Safe(() => __instance.ItemTemplate)
                ?? Safe(() => sourceProjectile?.Template);

            string weaponGuid = TemplateGuid(weaponTemplate);
            string ammunitionGuid = TemplateGuid(ammunitionTemplate);
            bool exactToolPair = string.Equals(
                    weaponGuid,
                    _tamingBowTemplateGuid,
                    StringComparison.OrdinalIgnoreCase)
                && string.Equals(
                    ammunitionGuid,
                    _tamingArrowTemplateGuid,
                    StringComparison.OrdinalIgnoreCase);

            // The acquisition lane begins only with the exact owner-supplied
            // Taming Bow and Taming Arrow identities. Every other projectile is
            // outside this companion-domain observation.
            if (!exactToolPair)
            {
                return;
            }

            EnsureSession();
            if (_rowCount >= MaxRowsPerGeneration)
            {
                if (!_rowLimitLogged)
                {
                    _rowLimitLogged = true;
                    _logger?.LogWarning($"Acquisition projectile probe reached its per-generation row cap of {MaxRowsPerGeneration}; further exact-pair contacts are ignored until the scene changes, the probe is toggled, or the plugin reloads.");
                }
                return;
            }

            Hero? currentHero = Hero.Current;
            ICharacter? owner = Safe(() => __instance.Owner);
            bool shooterIsCurrentHero = currentHero != null
                && !currentHero.HasBeenDiscarded
                && ReferenceEquals(owner, currentHero);

            NpcElement? targetNpc = __2 as NpcElement;
            Location? targetLocation = targetNpc?.ParentModel;
            string targetLocationId = SafeText(() => targetLocation?.ID);
            string targetTemplateGuid = SafeText(() => targetLocation?.Template?.GUID);
            string targetTemplateName = SafeText(() => targetLocation?.Template?.name);
            bool targetIsReviewedWolf = targetNpc != null
                && string.Equals(targetTemplateGuid, ReviewedWolfTemplateGuid, StringComparison.OrdinalIgnoreCase)
                && string.Equals(targetTemplateName, ReviewedWolfTemplateName, StringComparison.Ordinal);

            bool boundOnThisObservation = false;
            if (shooterIsCurrentHero
                && targetIsReviewedWolf
                && !string.IsNullOrWhiteSpace(targetLocationId)
                && string.IsNullOrWhiteSpace(_boundWolfLocationId)
                && (_bindFirstReviewedWolfHit?.Value ?? true))
            {
                _boundWolfLocationId = targetLocationId;
                _boundWolfActor = new WeakReference<NpcElement>(targetNpc!);
                boundOnThisObservation = true;
                _logger?.LogInfo(
                    "Acquisition projectile probe bound the current runtime-only wolf target from the exact Taming Tool pair: "
                    + $"scene={_sceneName}; generation={_sessionGeneration}; locationId={_boundWolfLocationId}; "
                    + $"template={targetTemplateName}[{targetTemplateGuid}].");
            }

            bool locationMatchesBoundTarget = targetIsReviewedWolf
                && !string.IsNullOrWhiteSpace(_boundWolfLocationId)
                && string.Equals(targetLocationId, _boundWolfLocationId, StringComparison.Ordinal);
            bool referenceMatchesBoundTarget = false;
            if (targetNpc != null
                && _boundWolfActor != null
                && _boundWolfActor.TryGetTarget(out NpcElement? boundActor))
            {
                referenceMatchesBoundTarget = ReferenceEquals(boundActor, targetNpc);
            }

            long sequence = ++_observationSequence;
            string outcome = ClassifyOutcome(
                shooterIsCurrentHero,
                __2,
                targetIsReviewedWolf,
                boundOnThisObservation,
                locationMatchesBoundTarget,
                referenceMatchesBoundTarget);
            string rejectionReason = ClassifyRejectionReason(
                shooterIsCurrentHero,
                __2,
                targetIsReviewedWolf,
                locationMatchesBoundTarget,
                referenceMatchesBoundTarget);

            string[] columns =
            {
                DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                sequence.ToString(CultureInfo.InvariantCulture),
                _sessionGeneration.ToString(CultureInfo.InvariantCulture),
                _sceneName,
                _toolSetStatus,
                _toolSetReason,
                Bool(exactToolPair),
                Bool(shooterIsCurrentHero),
                owner?.GetType().FullName ?? string.Empty,
                weaponGuid,
                TemplateName(weaponTemplate),
                TemplateItemName(weaponTemplate),
                ammunitionGuid,
                TemplateName(ammunitionTemplate),
                TemplateItemName(ammunitionTemplate),
                __instance.GetType().FullName ?? __instance.GetType().Name,
                SafeText(() => __1?.name),
                SafeInt(() => __1?.GetInstanceID() ?? 0).ToString(CultureInfo.InvariantCulture),
                __2?.GetType().FullName ?? string.Empty,
                Bool(__3),
                __0.Point.x.ToString("R", CultureInfo.InvariantCulture),
                __0.Point.y.ToString("R", CultureInfo.InvariantCulture),
                __0.Point.z.ToString("R", CultureInfo.InvariantCulture),
                targetLocationId,
                targetTemplateGuid,
                targetTemplateName,
                Bool(targetNpc != null),
                Bool(targetIsReviewedWolf),
                Bool(SafeBool(() => targetNpc?.IsUnique ?? false)),
                Bool(SafeBool(() => targetNpc?.IsSummonOrAlly ?? false)),
                Bool(SafeBool(() => targetNpc?.IsAlive ?? false)),
                Bool(SafeBool(() => targetNpc?.HasBeenDiscarded ?? false)),
                Bool(SafeBool(() => targetNpc?.IsUnconscious ?? false)),
                _boundWolfLocationId,
                Bool(boundOnThisObservation),
                Bool(locationMatchesBoundTarget),
                Bool(referenceMatchesBoundTarget),
                outcome,
                rejectionReason,
                "false",
                "false",
                "false",
                "false",
                "false",
            };

            _rowCount++;
            if (_writeCsv?.Value ?? true)
            {
                AppendRow(columns);
            }

            _logger?.LogInfo(
                "Acquisition projectile probe observed exact Taming Tool contact: "
                + $"sequence={sequence}; generation={_sessionGeneration}; scene={_sceneName}; "
                + $"heroShooter={shooterIsCurrentHero}; weapon={columns[10]}[{weaponGuid}]; "
                + $"ammo={columns[13]}[{ammunitionGuid}]; target={targetTemplateName}[{targetTemplateGuid}] "
                + $"location={targetLocationId}; boundMatch={locationMatchesBoundTarget}/{referenceMatchesBoundTarget}; "
                + $"outcome={outcome}; mutation=false.");
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Acquisition projectile probe contact observation failed closed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool TryRefreshTamingToolSet()
    {
        try
        {
            if (!Chainloader.PluginInfos.TryGetValue(TaintedWeaponsPluginGuid, out var pluginInfo)
                || pluginInfo.Instance == null)
            {
                SetToolReceipt("owner-unavailable", "tainted-weapons-plugin-not-loaded", registered: false);
                return false;
            }

            Assembly assembly = pluginInfo.Instance.GetType().Assembly;
            Type? apiType = assembly.GetType(TamingToolsApiTypeName, throwOnError: false);
            MethodInfo? ensure = apiType?.GetMethod(
                TamingToolsEnsureMethodName,
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                new[] { apiType.Assembly.GetType("TaintedWeapons.TaintedWeaponsTamingToolSetReceipt", throwOnError: false)?.MakeByRefType() ?? typeof(object).MakeByRefType() },
                modifiers: null);

            if (apiType == null)
            {
                SetToolReceipt("owner-api-unavailable", "taming-tools-api-type-missing", registered: false);
                return false;
            }

            // Resolve by name as a compatibility-safe fallback because the out
            // receipt type exists only in the optional owner assembly.
            ensure ??= apiType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(method =>
                {
                    if (!string.Equals(method.Name, TamingToolsEnsureMethodName, StringComparison.Ordinal))
                    {
                        return false;
                    }
                    ParameterInfo[] parameters = method.GetParameters();
                    return method.ReturnType == typeof(bool)
                        && parameters.Length == 1
                        && parameters[0].IsOut;
                });

            if (ensure == null)
            {
                SetToolReceipt("owner-api-unavailable", "ensure-registered-method-missing", registered: false);
                return false;
            }

            object?[] arguments = { null };
            bool accepted = ensure.Invoke(null, arguments) is bool result && result;
            object? receipt = arguments[0];
            if (receipt == null)
            {
                SetToolReceipt("owner-receipt-missing", "ensure-registered-returned-no-receipt", registered: false);
                return false;
            }

            string status = ReadStringProperty(receipt, "Status");
            string reason = ReadStringProperty(receipt, "ReasonCode");
            bool registered = ReadBoolProperty(receipt, "Registered");
            string bowGuid = ReadStringProperty(receipt, "TamingBowTemplateGuid");
            string bowName = ReadStringProperty(receipt, "TamingBowTemplateName");
            string arrowGuid = ReadStringProperty(receipt, "TamingArrowTemplateGuid");
            string arrowName = ReadStringProperty(receipt, "TamingArrowTemplateName");

            bool identityComplete = registered
                && !string.IsNullOrWhiteSpace(bowGuid)
                && !string.IsNullOrWhiteSpace(bowName)
                && !string.IsNullOrWhiteSpace(arrowGuid)
                && !string.IsNullOrWhiteSpace(arrowName);

            _toolSetStatus = string.IsNullOrWhiteSpace(status) ? "unknown" : status;
            _toolSetReason = string.IsNullOrWhiteSpace(reason) ? (accepted ? string.Empty : "owner-request-denied") : reason;
            _toolSetRegistered = identityComplete;
            if (identityComplete)
            {
                _tamingBowTemplateGuid = bowGuid;
                _tamingBowTemplateName = bowName;
                _tamingArrowTemplateGuid = arrowGuid;
                _tamingArrowTemplateName = arrowName;
            }

            LogToolReceipt(accepted, registered, bowGuid, bowName, arrowGuid, arrowName);
            return _toolSetRegistered;
        }
        catch (Exception ex)
        {
            SetToolReceipt("owner-call-failed", ex.GetType().Name + ":" + ex.Message, registered: false);
            return false;
        }
    }

    private static void SetToolReceipt(string status, string reason, bool registered)
    {
        _toolSetStatus = status;
        _toolSetReason = reason;
        _toolSetRegistered = registered;
        LogToolReceipt(
            accepted: false,
            registered,
            _tamingBowTemplateGuid,
            _tamingBowTemplateName,
            _tamingArrowTemplateGuid,
            _tamingArrowTemplateName);
    }

    private static void LogToolReceipt(
        bool accepted,
        bool registered,
        string bowGuid,
        string bowName,
        string arrowGuid,
        string arrowName)
    {
        string row = $"status={_toolSetStatus}; reason={_toolSetReason}; accepted={accepted}; registered={registered}; "
            + $"bow={bowName}[{bowGuid}]; arrow={arrowName}[{arrowGuid}]";
        if (string.Equals(row, _lastToolReceiptLog, StringComparison.Ordinal))
        {
            return;
        }

        _lastToolReceiptLog = row;
        if (registered)
        {
            _logger?.LogInfo("Acquisition projectile probe consumed Tainted Weapons Taming Tool receipt; " + row);
        }
        else
        {
            _logger?.LogWarning("Acquisition projectile probe waiting for Tainted Weapons Taming Tool receipt; " + row);
        }
    }

    private static string ReadStringProperty(object instance, string propertyName)
    {
        try
        {
            return instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(instance) as string
                ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool ReadBoolProperty(object instance, string propertyName)
    {
        try
        {
            return instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(instance) is bool value
                && value;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsEnabled => _applied
        && (_enabled?.Value ?? false)
        && Plugin.Enabled.Value;

    private static void OnEnabledSettingChanged(object? sender, EventArgs e)
    {
        if (!_applied)
        {
            return;
        }

        ClearToolIdentity();
        StartSession(
            ActiveSceneName(),
            _enabled?.Value == true ? "probe-enabled" : "probe-disabled");
    }

    private static void EnsureSession()
    {
        string activeScene = ActiveSceneName();
        if (!_sessionInitialized
            || !string.Equals(_sceneName, activeScene, StringComparison.Ordinal))
        {
            StartSession(activeScene, "contact-session");
        }
    }

    private static void OnActiveSceneChanged(Scene previous, Scene next)
    {
        if (_applied)
        {
            StartSession(next.name ?? string.Empty, "active-scene-changed");
        }
    }

    private static void StartSession(string sceneName, string reason)
    {
        _sessionGeneration++;
        _sessionInitialized = true;
        _sceneName = sceneName ?? string.Empty;
        _rowCount = 0;
        _observationSequence = 0;
        _rowLimitLogged = false;
        _boundWolfLocationId = string.Empty;
        _boundWolfActor = null;

        if (IsEnabled)
        {
            _logger?.LogInfo(
                "Acquisition projectile probe session reset: "
                + $"generation={_sessionGeneration}; scene={_sceneName}; reason={reason}; boundWolfCleared=true; exactToolPairRequired=true.");
        }
    }

    private static string ActiveSceneName()
    {
        try
        {
            return SceneManager.GetActiveScene().name ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ClassifyOutcome(
        bool shooterIsCurrentHero,
        IAlive? aliveHit,
        bool targetIsReviewedWolf,
        bool boundOnThisObservation,
        bool locationMatchesBoundTarget,
        bool referenceMatchesBoundTarget)
    {
        if (aliveHit == null)
        {
            return "taming-tool-environment-contact";
        }
        if (!shooterIsCurrentHero)
        {
            return "taming-tool-non-player-projectile";
        }
        if (!targetIsReviewedWolf)
        {
            return "taming-tool-other-living-target";
        }
        if (boundOnThisObservation)
        {
            return "reviewed-wolf-bound";
        }
        if (locationMatchesBoundTarget && referenceMatchesBoundTarget)
        {
            return "bound-wolf-match";
        }
        if (locationMatchesBoundTarget)
        {
            return "bound-location-reference-mismatch";
        }
        return "other-reviewed-wolf";
    }

    private static string ClassifyRejectionReason(
        bool shooterIsCurrentHero,
        IAlive? aliveHit,
        bool targetIsReviewedWolf,
        bool locationMatchesBoundTarget,
        bool referenceMatchesBoundTarget)
    {
        if (!shooterIsCurrentHero)
        {
            return "shooter-not-current-hero";
        }
        if (aliveHit == null)
        {
            return "no-living-target";
        }
        if (!targetIsReviewedWolf)
        {
            return "target-not-reviewed-wolf";
        }
        if (string.IsNullOrWhiteSpace(_boundWolfLocationId))
        {
            return "no-runtime-wolf-bound";
        }
        if (!locationMatchesBoundTarget)
        {
            return "location-id-does-not-match-bound-wolf";
        }
        if (!referenceMatchesBoundTarget)
        {
            return "npc-reference-does-not-match-bound-wolf";
        }
        return string.Empty;
    }

    private static void AppendRow(string[] columns)
    {
        lock (FileGate)
        {
            string folder = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, CsvFileName);
            bool writeHeader = !File.Exists(path) || new FileInfo(path).Length == 0;

            var builder = new StringBuilder();
            if (writeHeader)
            {
                builder.AppendLine(CsvHeader);
            }
            builder.AppendLine(string.Join(",", Array.ConvertAll(columns, Escape)));
            File.AppendAllText(path, builder.ToString(), Encoding.UTF8);
        }
    }

    private static string TemplateGuid(ItemTemplate? template)
    {
        return SafeText(() => template?.GUID);
    }

    private static string TemplateName(ItemTemplate? template)
    {
        return SafeText(() => template?.name);
    }

    private static string TemplateItemName(ItemTemplate? template)
    {
        return SafeText(() => template?.ItemName);
    }

    private static T? Safe<T>(Func<T?> getter) where T : class
    {
        try
        {
            return getter();
        }
        catch
        {
            return null;
        }
    }

    private static string SafeText(Func<string?> getter)
    {
        try
        {
            return getter() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool SafeBool(Func<bool> getter)
    {
        try
        {
            return getter();
        }
        catch
        {
            return false;
        }
    }

    private static int SafeInt(Func<int> getter)
    {
        try
        {
            return getter();
        }
        catch
        {
            return 0;
        }
    }

    private static string Bool(bool value)
    {
        return value ? "true" : "false";
    }

    private static string Escape(string value)
    {
        string text = value ?? string.Empty;
        if (text.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
        {
            return text;
        }
        return '"' + text.Replace("\"", "\"\"") + '"';
    }

    private static void ClearToolIdentity()
    {
        _nextToolRegistrationAttemptAt = 0f;
        _toolSetStatus = "not-requested";
        _toolSetReason = string.Empty;
        _toolSetRegistered = false;
        _tamingBowTemplateGuid = string.Empty;
        _tamingBowTemplateName = string.Empty;
        _tamingArrowTemplateGuid = string.Empty;
        _tamingArrowTemplateName = string.Empty;
        _lastToolReceiptLog = string.Empty;
    }

    private static void BeforePluginDestroy()
    {
        Shutdown();
    }

    private static void Shutdown()
    {
        if (_sceneEventSubscribed)
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            _sceneEventSubscribed = false;
        }
        if (_enabled != null)
        {
            _enabled.SettingChanged -= OnEnabledSettingChanged;
        }

        _applied = false;
        _sessionInitialized = false;
        _sceneName = string.Empty;
        _boundWolfLocationId = string.Empty;
        _boundWolfActor = null;
        _rowCount = 0;
        _observationSequence = 0;
        _rowLimitLogged = false;
        ClearToolIdentity();
        _enabled = null;
        _writeCsv = null;
        _bindFirstReviewedWolfHit = null;
        _logger = null;
    }
}
