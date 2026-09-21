# Creature Pipeline Validation Matrix

| Gate | Required proof |
| --- | --- |
| CI1 source | Exact authorised source revision recorded |
| CI2 baseline | Exact native contract and build scope recorded |
| CI3 visuals | Pack-owned visual root loads/releases repeatedly |
| CI4A animation | Required native states explicitly mapped |
| CI4 templates | Pack-owned NpcTemplate and LocationTemplate resolve |
| CI5 construction | Controlled actor created through intended Location/Npc owner |
| Controller | Grounding/movement contract works |
| AI/targeting | Native recognition/target behaviour works |
| Combat | Required attacks and damage path work |
| Hit reaction | Required hit state works |
| Death | Lethal transition and Death state work |
| Corpse | Native NpcDummy/Corpse handoff preserved |
| Cleanup | Exact actor/location cleanup works without deleting native handoff early |
| Placement | Selected distribution owner verified |
| Duplicate prevention | Repeated triggers do not create unintended duplicates |
| Persistence | Cold save/load only if claimed |
| Missing package | Explicit behaviour only if claimed |
| Migration | Upgrade path only if claimed |

Every row is independent.
