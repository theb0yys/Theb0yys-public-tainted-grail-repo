using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Combat;
using Awaken.TG.Main.Heroes.MovementSystems;
using Awaken.TG.Main.Utility;
using Awaken.TG.MVC;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGExample.ExtraAirJump;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.extra-air-jump";
    public const string PluginName = "TG Example - Extra Air Jump";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<float> _heightMultiplier = null!;
    private Harmony? _harmony;

    internal static bool Enabled => s_instance?._enabled.Value ?? false;
    internal static float HeightMultiplier => s_instance == null ? 1f : Mathf.Clamp(s_instance._heightMultiplier.Value, 0.25f, 3f);

    private void Awake()
    {
        s_instance = this;
        _enabled = Config.Bind("General", "Enabled", true, "Allow one additional airborne jump.");
        _heightMultiplier = Config.Bind(
            "Jump",
            "ExtraJumpHeightMultiplier",
            1f,
            new ConfigDescription("Height multiplier for only the extra airborne jump.",
                new AcceptableValueRange<float>(0.25f, 3f)));

        _harmony = new Harmony(PluginGuid);
        ExtraJumpPatch.Apply(_harmony, Logger);

        Logger.LogInfo($"{PluginName} loaded. Multiplier={HeightMultiplier:0.##}");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }
}

internal static class ExtraJumpPatch
{
    private static readonly FieldInfo? ControllerField =
        AccessTools.Field(typeof(HeroMovementSystem), "<Controller>k__BackingField");

    private static readonly ConditionalWeakTable<HumanoidMovementBase, State> States = new();

    internal static void Apply(Harmony harmony, BepInEx.Logging.ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(HumanoidMovementBase), nameof(HumanoidMovementBase.Update), new[] { typeof(float) });
        MethodInfo? prefix = AccessTools.Method(typeof(ExtraJumpPatch), nameof(Prefix));
        MethodInfo? postfix = AccessTools.Method(typeof(ExtraJumpPatch), nameof(Postfix));

        if (target == null || prefix == null || postfix == null || ControllerField == null)
        {
            logger.LogWarning("HumanoidMovementBase.Update or controller backing field was not found.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));
        logger.LogInfo("Patched HumanoidMovementBase.Update for the extra-air-jump example.");
    }

    private static void Prefix(HumanoidMovementBase __instance, out bool __state)
    {
        VHeroController? controller = GetController(__instance);
        __state = controller != null && controller.Grounded;
    }

    private static void Postfix(HumanoidMovementBase __instance, bool __state)
    {
        VHeroController? controller = GetController(__instance);
        if (controller == null)
        {
            return;
        }

        State state = States.GetOrCreateValue(__instance);

        if (!Plugin.Enabled || controller.Grounded || controller.IsSwimming)
        {
            state.Available = true;
            return;
        }

        Hero? hero = controller.Target;
        if (hero == null || hero.HasBeenDiscarded || !ReferenceEquals(hero, Hero.Current))
        {
            return;
        }

        if (__state || !state.Available || __instance.IsPerformingAction || hero.IsPerformingAction)
        {
            return;
        }

        if (controller.IsCrouching && !controller.CanStandUp)
        {
            return;
        }

        if (controller.Input == null || !controller.Input.GetButtonDown(KeyBindings.Gameplay.Jump))
        {
            return;
        }

        ApplyJump(controller, hero, state);
    }

    private static VHeroController? GetController(HumanoidMovementBase movement)
    {
        return ControllerField?.GetValue(movement) as VHeroController;
    }

    private static void ApplyJump(VHeroController controller, Hero hero, State state)
    {
        float gravity = controller.Data.gravity;
        if (gravity >= -0.0001f)
        {
            return;
        }

        float jumpHeight = Mathf.Max(0.01f, (float)hero.HeroStats.JumpHeight * Plugin.HeightMultiplier);
        float targetVerticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        controller.Grounded = false;
        controller.OnHeroJumped();
        controller.verticalVelocity = Mathf.Max(controller.verticalVelocity, targetVerticalVelocity);
        ModelExtensions.Trigger(hero, Hero.Events.HeroJumped, payload: true);

        state.Available = false;
    }

    private sealed class State
    {
        internal bool Available = true;
    }
}
