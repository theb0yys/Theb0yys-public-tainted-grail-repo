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

namespace TGCommunity.Example.TemporaryHumanAlly;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.temporary-human-ally";
    public const string PluginName = "TG Example - Temporary Human Ally";
    public const string PluginVersion = "0.1.0";

    private const string TemplateName = "Spec_NPC_Special_GalahadSquire_Repetetive";
    private const string TemplateGuid = "a13a2abd2f5e61d438f322360035ea9a";

    private Location? _owned;

    private void Awake()
    {
        Logger.LogInfo($"{PluginName} loaded. F8 summon/dismiss; F9 request native defend.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            if (HasOwned())
            {
                Dismiss();
            }
            else
            {
                Summon();
            }
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            Defend();
        }

        if (HasOwned())
        {
            _owned!.MarkedNotSaved = true;
        }
    }

    private void OnDestroy()
    {
        Dismiss();
    }

    private void Summon()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            Logger.LogWarning("Hero.Current unavailable.");
            return;
        }

        LocationTemplate? template;
        try
        {
            template = new TemplateReference(TemplateGuid).Get<LocationTemplate>();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Template lookup failed: {ex.GetType().Name}: {ex.Message}");
            return;
        }

        if (template == null ||
            !string.Equals(template.GUID, TemplateGuid, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(template.name, TemplateName, StringComparison.Ordinal))
        {
            Logger.LogWarning("Exact repetitive Galahad Squire template did not resolve.");
            return;
        }

        NpcAttachment? attachment = template.GetComponent<NpcAttachment>();
        if (attachment == null || attachment.IsUnique)
        {
            Logger.LogWarning("Template is missing NpcAttachment or is unique.");
            return;
        }

        Vector3 spawn = hero.Coords + hero.Rotation * ((Vector3.back * 3f) + (Vector3.right * 1.6f));
        Location location = template.SpawnLocation(spawn, hero.Rotation);
        location.MarkedNotSaved = true;

        if (!location.TryGetElement(out NpcElement npc) || npc == null || npc.IsUnique)
        {
            location.MarkedNotSaved = true;
            location.Discard();
            Logger.LogWarning("Spawned Location did not produce a safe non-unique NpcElement.");
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
                Logger.LogWarning("Native ally marker could not be established.");
                return;
            }
        }
        catch (Exception ex)
        {
            location.MarkedNotSaved = true;
            location.Discard();
            Logger.LogWarning($"Native ally setup failed: {ex.GetType().Name}: {ex.Message}");
            return;
        }

        _owned = location;
        Logger.LogInfo($"Temporary human ally active: {TemplateName}[{TemplateGuid}], location={location.ID}.");
    }

    private void Defend()
    {
        if (!TryGetOwnedNpc(out NpcElement? npc) || npc == null)
        {
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null || CountLiveAttackers(hero) == 0)
        {
            Logger.LogInfo("Defend blocked: Hero has no live attackers.");
            return;
        }

        NpcHeroPetAlly? ally = npc.TryGetElement<NpcHeroPetAlly>();
        if (ally == null || ally.HasBeenDiscarded)
        {
            return;
        }

        _owned!.MarkedNotSaved = true;
        ally.EnterCombat();
        Logger.LogInfo("NpcHeroPetAlly.EnterCombat() requested.");
    }

    private static int CountLiveAttackers(Hero hero)
    {
        int count = 0;
        foreach (ICharacter attacker in hero.PossibleAttackers)
        {
            if (attacker != null && !attacker.WasDiscarded && attacker.IsAlive)
            {
                count++;
            }
        }

        return count;
    }

    private bool TryGetOwnedNpc(out NpcElement? npc)
    {
        npc = null;
        return HasOwned() && _owned!.TryGetElement(out npc) && npc != null && !npc.HasBeenDiscarded;
    }

    private bool HasOwned()
    {
        if (_owned == null)
        {
            return false;
        }

        if (_owned.HasBeenDiscarded)
        {
            _owned = null;
            return false;
        }

        return true;
    }

    private void Dismiss()
    {
        Location? owned = _owned;
        _owned = null;

        if (owned != null && !owned.HasBeenDiscarded)
        {
            owned.MarkedNotSaved = true;
            owned.Discard();
            Logger.LogInfo("Temporary human ally discarded.");
        }
    }
}
