# Build a Sidecar Map Pinbook

Store personal location notes under BepInEx config using Hero.Current.Coords. Do not create native map markers or edit FoA discovery memory.

Working lineage: [Sidecar Pinbook Instead of Native Map Injection](../../../research/case-studies/map/mod-owned-pinbook.md).

## Position source

Use the current hero first:

~~~csharp
Hero hero = Hero.Current;
Vector3 position = hero.Coords;
~~~

The maintained implementation can use a player-transform fallback when needed, but Hero.Current.Coords is the primary source.

## Storage file

The working mod uses:

~~~text
StorageFileName = PluginGuid + ".pins.tsv"
_storagePath = Path.Combine(Paths.ConfigPath, StorageFileName)
StorageVersion = 1
~~~

A stored row contains:

~~~text
v1
base64-encoded name
X
Y
Z
~~~

separated by tabs.

Coordinates are formatted with invariant culture and round-trip float formatting.

## Capture

On the save-pin action:

1. require gameplay/hero availability;
2. require current pin count below the configured limit;
3. capture current position;
4. open a mod-owned naming UI;
5. sanitize the name;
6. append a PinNote to the mod-owned list;
7. persist the list.

No FoA map object needs to be created.

## Durable write pattern

The working implementation writes the whole pin list to:

~~~text
<storage>.tmp
~~~

Then:

- if the final file exists, File.Replace(temp, final, backup);
- remove the temporary backup after success;
- otherwise File.Move(temp, final).

If writing fails, delete the .tmp file and keep the session copy in memory.

## Load pattern

At startup:

~~~text
File.ReadAllLines(storage)
→ parse each v1 row
→ ignore/report malformed non-empty rows
→ stop adding when MaxStoredPins is reached
~~~

A malformed line does not affect native game data.

## HUD calculations

For a compact pin HUD:

~~~csharp
float distance = Vector3.Distance(currentPosition, pin.Position);
~~~

The maintained mod also builds a local forward/right direction basis and can sort pin display by squared distance.

## UI ownership

The pin manager/naming screen is mod-owned UI.

When opened, acquire the appropriate cursor/input scope; on close, disable, or destroy, restore the previous cursor/input state.

The pinbook never owns native discovery, fast travel, map fog, or quest markers.
