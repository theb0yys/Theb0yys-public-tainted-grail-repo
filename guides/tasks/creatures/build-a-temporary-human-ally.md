# Build a Temporary Human Ally

Use a non-unique human LocationTemplate, spawn a new Location, mark it session-only, then attach FoA's existing summon/ally ownership.

Working lineage: [Human Native-Ally Proof](../../../research/case-studies/companions/native-human-ally-proof.md).

## Runnable source

Start with the [Temporary human ally example](../../../examples/mono/gameplay/temporary-human-ally/README.md). Build it unchanged first, then change one setting or mechanism at a time.

## Pick a non-unique template

Resolve the configured GUID through:

~~~csharp
LocationTemplate template =
    new TemplateReference(guid).Get<LocationTemplate>();
~~~

Before spawning, require:

~~~text
template has NpcAttachment
NpcAttachment.IsUnique == false
~~~

After spawning, also require:

~~~text
NpcElement exists
NpcElement.IsUnique == false
~~~

Do not use this process on named story/quest NPCs.

One reviewed repetitive human candidate in the maintained roster is:

~~~text
Spec_NPC_Special_GalahadSquire_Repetetive
a13a2abd2f5e61d438f322360035ea9a
~~~

## Spawn beside the hero

The working route uses a local-space offset from the hero:

~~~csharp
Vector3 offset = (Vector3.back * distance) + (Vector3.right * 1.6f);
Vector3 spawnPosition = hero.Coords + hero.Rotation * offset;

Location location =
    template.SpawnLocation(spawnPosition, hero.Rotation);

location.MarkedNotSaved = true;
~~~

If NpcElement validation fails, mark the Location not saved and discard it immediately.

## Convert the new actor to native ally ownership

For the spawned NpcElement:

~~~csharp
npc.OverrideFaction(
    hero.GetFactionTemplateForSummon(),
    FactionOverrideContext.Summon);

npc.AddElement(new NpcHeroPetAlly(hero));
~~~

Then read the marker back with TryGetElement<NpcHeroPetAlly>() and fail closed if it is missing/discarded.

Keep Location.MarkedNotSaved = true.

## Native defend handoff

When the hero has live attackers and the companion has a valid NpcHeroPetAlly:

~~~csharp
marker.EnterCombat();
~~~

The working implementation counts Hero.PossibleAttackers and requires live attackers before requesting the defend handoff.

Do not choose combat targets yourself.

## Optional runtime commands

The maintained implementation attaches session-only Location elements for actions such as:

- follow;
- hold;
- defend;
- come close;
- recall;
- dismiss.

Each action element is MarkedNotSaved.

Those actions should manipulate only the owned companion.

## Dismissal

Keep the exact owned Location reference.

On dismiss, failure, swap, or plug-in shutdown:

~~~text
mark Location not saved
→ remove owned command elements as needed
→ Location.Discard()
→ clear local reference
~~~

Do not recruit or mutate an existing world NPC in place.
