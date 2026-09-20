# Runtime Access Reference

Exact lookup for obtaining important runtime owners and objects.

Access to an object does not by itself prove gameplay, persistence or lifecycle ownership. For ownership, use [Systems](../../systems/README.md).

## Publicly documented access patterns

| Subject | Public access surface | What the public evidence establishes | Scope / caveat |
| --- | --- | --- | --- |
| Hero-owned items | `HeroItems.OnRestore` | The instance received as `this` is valid at restore time and can be captured for later use | Demonstrated by an IL2CPP native mod; this is an availability point, not proof that a cached pointer remains valid forever |
| Known item state | `KnownItems` | Existing known-item state can be scanned after the player/session is loaded | Public Mono and IL2CPP mods use it for startup reconstruction/backfill |
| Recipe learning | `HeroRecipes.LearnRecipe` | Native known recipes can be issued through the hero recipe owner | Public IL2CPP implementation; does not establish custom-recipe registration |
| UI hosting | `Services.Get<ViewHosting>().OnMainCanvas()` | Questline's public Merlin source resolves `ViewHosting` through the MVC service locator and asks it for the main canvas host | Merlin/public-source evidence; do not assume every runtime surface exposes the identical managed call path |
| Hero storage | `HeroStorage.Items` | Public mod documentation reads the native hero-storage item collection while the Hero Storage interface is open | Availability outside that context is not established here |

## Availability matters

Public mod evidence distinguishes plugin load from usable gameplay state. One public recipe mod waits until the player is loaded before scanning `KnownItems`. Another moved IL2CPP `HeroItems` acquisition to `HeroItems.OnRestore` after a generic element lookup / field-offset approach proved unsafe.

Treat acquisition as a lifecycle question:

~~~text
plugin loaded
→ required owner exists
→ owner restored/initialized
→ operation is safe
~~~

## Unsafe or misleading access patterns

- Do not treat a successful pointer/type lookup as proof that the object is currently valid.
- A public IL2CPP implementation reported stale/freed entries while traversing `CraftingTemplate.recipes` (`TemplateReference[]`); pointer/class checks alone were insufficient.
- A cached native pointer needs its own lifetime proof.
- Access to a presentation object does not prove ownership of the underlying gameplay state.

## Evidence provenance

This page currently records public evidence from Questline's public Merlin Workshop source and publicly released FoA mods. Exact signatures, assembly ownership and cross-runtime equivalence should be added only when independently established.
