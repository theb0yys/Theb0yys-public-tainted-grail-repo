using System;
using System.Reflection;
using Awaken.TG.Main.Animations.FSM.Heroes.Machines;
using Awaken.TG.Main.Animations.FSM.Heroes.States.Bow;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Combat;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Heroes.Items.Attachments;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TGExample.AttackCastActionObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.attack-cast-action-observer";
    public const string PluginName = "TG Example - Attack/Cast Action Observer";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource? LogSource { get; private set; }

    private Harmony? _harmony;

    private void Awake()
    {
        LogSource = Logger;
        _harmony = new Harmony(PluginGuid);
        ActionObserverPatch.Apply(_harmony, Logger);
        Logger.LogInfo("${PluginName} loaded. This example observes native action lifecycle seams only.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        LogSource = null;
    }
}

internal static class ActionObserverPatch
{
    private static readonly FieldInfo? AttachedToHeroField =
        AccessTools.Field(typeof(CharacterWeapon), "_attachedToHero");

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        PatchPostfix(harmony, logger,
            AccessTools.Method(typeof(CharacterWeapon), "AttackBegun"),
            nameof(MeleeStartPostfix),
            "CharacterWeapon.AttackBegun");

        PatchPostfix(harmony, logger,
            AccessTools.Method(typeof(CharacterWeapon), "AttackEnded"),
            nameof(MeleeEndPostfix),
            "CharacterWeapon.AttackEnded");

        PatchPostfix(harmony, logger,
            AccessTools.Method(typeof(BowPull), "AfterEnter", new[] { typeof(float) }),
            nameof(BowDrawStartPostfix),
            "BowPull.AfterEnter");

        PatchPostfix(harmony, logger,
            AccessTools.Method(typeof(BowFSM), "EndBowDrawState"),
            nameof(BowDrawEndPostfix),
            "BowFSM.EndBowDrawState");

        PatchPostfix(harmony, logger,
            AccessTools.Method(typeof(Item), "StartPerforming", new[] { typeof(ItemActionType) }),
            nameof(CastStartPostfix),
            "Item.StartPerforming");

        PatchPostfix(harmony, logger,
            AccessTools.Method(typeof(Item), "EndPerforming", new[] { typeof(ItemActionType) }),
            nameof(CastEndPostfix),
            "Item.EndPerforming");

        PatchPostfix(harmony, logger,
            AccessTools.Method(typeof(Item), "CancelPerforming", new[] { typeof(ItemActionType) }),
            nameof(CastCancelPostfix),
            "Item.CancelPerforming");
    }

    private static void PatchPostfix(
        Harmony harmony,
        ManualLogSource logger,
        MethodInfo? target,
        string postfixName,
        string description)
    {
        MethodInfo? postfix = AccessTools.Method(typeof(ActionObserverPatch), postfixName);
        if (target == null || postfix == null)
        {
            logger.LogWarning("${description} observer target was not found.");
            return;
        }

        harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        logger.LogInfo("$Patched {description} for read-only observation.");
    }

    private static void MeleeStartPostfix(CharacterWeapon __instance)
    {
        if (!IsHeroMeleeWeapon(__instance))
        {
            return;
        }

        Plugin.LogSource?.LogInfo("$Action lifecycle: family=melee; event=start; context={HandContext()}");
    }

    private static void MeleeEndPostfix(CharacterWeapon __instance)
    {
        if (!IsHeroMeleeWeapon(__instance))
        {
            return;
        }

        Plugin.LogSource?.LogInfo("$Action lifecycle: family=melee; event=end; context={HandContext()}");
    }

    private static void BowDrawStartPostfix()
    {
        Hero? hero = CurrentHero();
        if (hero == null)
        {
            return;
        }

        Plugin.LogSource?.LogInfo(
            "$Action lifecycle: family=ranged; event=draw-start; pullingRanged={SafeBool(() => hero.PullingRangedWeapon)}; " +
            "$context={HandContext()}");
    }

    private static void BowDrawEndPostfix()
    {
        Hero? hero = CurrentHero();
        if (hero == null)
        {
            return;
        }

        Plugin.LogSource?.LogInfo(
            "$Action lifecycle: family=ranged; event=draw-end; pullingRanged={SafeBool(() => hero.PullingRangedWeapon)}; " +
            "$context={HandContext()}");
    }

    private static void CastStartPostfix(Item __instance, ItemActionType __0)
    {
        LogCast("start", __instance, __0);
    }

    private static void CastEndPostfix(Item __instance, ItemActionType __0)
    {
        LogCast("end", __instance, __0);
    }

    private static void CastCancelPostfix(Item __instance, ItemActionType __0)
    {
        LogCast("cancel-path", __instance, __0);
    }

    private static void LogCast(string eventName, Item item, ItemActionType actionType)
    {
        if (!IsCastSpell(actionType))
        {
            return;
        }

        Hero? hero = CurrentHero();
        if (hero == null || item == null || !ReferenceEquals(item.Character, hero))
        {
            return;
        }

        Plugin.LogSource?.LogInfo(
            "$Action lifecycle: family=spell; event={eventName}; item={ItemName(item)}; context={HandContext()}");
    }

    private static bool IsHeroMeleeWeapon(CharacterWeapon weapon)
    {
        Hero? hero = CurrentHero();
        if (hero == null || weapon == null)
        {
            return false;
        }

        try
        {
            if (AttachedToHeroField?.GetValue(weapon) is not bool attached || !attached)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        return IsMelee(SafeItem(() => hero.MainHandItem))
            || IsMelee(SafeItem(() => hero.OffHandItem));
    }

    private static bool IsMelee(Item? item)
    {
        try
        {
            return item?.Template?.IsMelee == true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsCastSpell(ItemActionType? actionType)
    {
        if (actionType == null)
        {
            return false;
        }

        try
        {
            return ReferenceEquals(actionType, ItemActionType.CastSpell)
                || actionType.Equals(ItemActionType.CastSpell);
        }
        catch
        {
            return false;
        }
    }

    private static Hero? CurrentHero()
    {
        Hero? hero = Hero.Current;
        return hero == null || hero.HasBeenDiscarded ? null : hero;
    }

    private static string HandContext()
    {
        Hero? hero = CurrentHero();
        if (hero == null)
        {
            return "hero-unavailable";
        }

        return "$main={ItemName(SafeItem(() => hero.MainHandItem))},off={ItemName(SafeItem(() => hero.OffHandItem))}";
    }

    private static Item? SafeItem(Func<Item?> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return null;
        }
    }

    private static bool SafeBool(Func<bool> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return false;
        }
    }

    private static string ItemName(Item? item)
    {
        if (item == null)
        {
            return "none";
        }

        try
        {
            return string.IsNullOrWhiteSpace(item.DisplayName)
                ? item.Template?.ItemName ?? item.GetType().Name
                : item.DisplayName;
        }
        catch
        {
            return item.GetType().Name;
        }
    }
}
