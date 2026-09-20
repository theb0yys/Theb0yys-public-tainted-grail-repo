using System;
using Awaken.TG.Main.AI.SummonsAndAllies;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights;
using Awaken.TG.Main.Fights.Factions;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Templates;
using BepInEx;
using UnityEngine;

namespace TGCommunity.Example.NativeSessionCompanion;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.native-session-companion";
    public const string PluginName = "TG Example - Native Session Companion";
    public const string PluginVersion = "0.1.0";

    private const string QrkoTemplateGuid = "cc30c92e0699e3c41be92d1e99057354";
    private const string QrkoTemplateName = "Spec_Pet_Qrko";

    private Location? _active;
    private float _nextSafetyTick;

    private void Awake()
    {
        Logger.LogInfo($"{PluginName} loaded. F8 summon/dismiss exact Qrko; F9 requests native defend handoff when the hero has a live attacker.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (HasActive())
            {
                Dismiss("player-command");
            }
            else
            {
                Summon();
            }
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            RequestDefend();
        }

        if (Time.unscaledTime >= _nextSafetyTick)
        {
            _nextSafetyTick = Time.unscaledTime + 1f;
            if (HasActive())
            {
                _active!.MarkedNotSaved = true;
            }
        }
    }

    private void OnDestroy()
    {
        Dismiss("plugin-unload");
    }

    private void Summon()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            Logger.LogWarning("Summon blocked: Hero.Current unavailable.");
            return;
        }

        LocationTemplate? template;
        try
        {
            template = new TemplateReference(QrkoTemplateGuid).Get<LocationTemplate>();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Summon blocked: template lookup failed ({ex.GetType().Name}).");
            return;
        }

        if (template == null ||
            !string.Equals(template.GUID, QrkoTemplateGuid, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(template.name, QrkoTemplateName, StringComparison.Ordinal))
        {
            Logger.LogWarning("Summon blocked: exact Qrko LocationTemplate did not resolve.");
            return;
        }

        Vector3 spawnPosition = hero.Coords + hero.Rotation * (Vector3.right * 1.75f);

        Location location;
        try
        {
            location = template.SpawnLocation(spawnPosition, hero.Rotation);
            location.MarkedNotSaved = true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Summon failed: {ex.GetType().Name}: {ex.Message}");
            return;
        }

        if (!location.TryGetElement(out NpcElement npc) || npc == null)
        {
            location.MarkedNotSaved = true;
            location.Discard();
            Logger.LogWarning("Summon failed: spawned Qrko has no NpcElement.");
            return;
        }

        try
        {
            npc.OverrideFaction(hero.GetFactionTemplateForSummon(), FactionOverrideContext.Summon);
            if (!npc.HasElement<NpcHeroPetAlly>())
            {
                npc.AddElement(new NpcHeroPetAlly(hero));
            }

            NpcHeroPetAlly? ally = npc.TryGetElement<NpcHeroPetAlly>();
            if (ally == null || ally.HasBeenDiscarded)
            {
                location.MarkedNotSaved = true;
                location.Discard();
                Logger.LogWarning("Summon failed: native ally marker readback failed.");
                return;
            }
        }
        catch (Exception ex)
        {
            location.MarkedNotSaved = true;
            location.Discard();
            Logger.LogWarning($"Summon failed during native ally setup: {ex.GetType().Name}: {ex.Message}");
            return;
        }

        _active = location;
        Logger.LogInfo($"One-session Qrko active. location={location.ID}; template={template.name}[{template.GUID}]; MarkedNotSaved=true; nativeAlly=true.");
    }

    private void RequestDefend()
    {
        if (!TryGetActiveNpc(out NpcElement? npc) || npc == null)
        {
            Logger.LogInfo("Defend blocked: no active managed Qrko.");
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null || CountLiveAttackers(hero) == 0)
        {
            Logger.LogInfo("Defend blocked: hero has no live attacker.");
            return;
        }

        NpcHeroPetAlly? ally = npc.TryGetElement<NpcHeroPetAlly>();
        if (ally == null || ally.HasBeenDiscarded)
        {
            Logger.LogWarning("Defend blocked: native NpcHeroPetAlly marker is unavailable.");
            return;
        }

        _active!.MarkedNotSaved = true;
        ally.EnterCombat();
        Logger.LogInfo("Native defend handoff requested through NpcHeroPetAlly.EnterCombat().");
    }

    private static int CountLiveAttackers(Hero hero)
    {
        int count = 0;
        try
        {
            foreach (ICharacter attacker in hero.PossibleAttackers)
            {
                if (attacker != null && !attacker.WasDiscarded && attacker.IsAlive)
                {
                    count++;
                }
            }
        }
        catch
        {
            return 0;
        }

        return count;
    }

    private bool TryGetActiveNpc(out NpcElement? npc)
    {
        npc = null;
        if (!HasActive())
        {
            return false;
        }

        return _active!.TryGetElement(out npc) && npc != null && !npc.HasBeenDiscarded;
    }

    private bool HasActive()
    {
        if (_active == null)
        {
            return false;
        }

        if (_active.HasBeenDiscarded)
        {
            _active = null;
            return false;
        }

        return true;
    }

    private void Dismiss(string reason)
    {
        if (_active == null)
        {
            return;
        }

        Location owned = _active;
        _active = null;

        if (!owned.HasBeenDiscarded)
        {
            owned.MarkedNotSaved = true;
            owned.Discard();
            Logger.LogInfo($"Owned Qrko dismissed through Location.Discard(). reason={reason}");
        }
    }
}
