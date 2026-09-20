# Merlin Basic Overlay

This is a concrete starter **to overlay into the official Merlin Workshop project**, not a standalone Unity project.

Upstream: https://github.com/AR-Questline/merlin-workshop

The current upstream project contains the Unity project, `Assets/ModInfo.asset`, code/tooling, packages and project settings. Keep those upstream-owned surfaces in the Merlin project. Put your own authored content under a clearly owned root such as:

```text
Assets/
  CommunityMod/
    Data/
    Prefabs/
    Scenes/
    Scripts/
```

This template includes `Assets/CommunityMod/README.md` so the owned root exists when copied.

## Use

1. Fork/clone or otherwise obtain the official Merlin Workshop project using its documented installation route.
2. Copy this template's `Assets/CommunityMod/` folder into that project.
3. Rename `CommunityMod` to your mod/content identity before creating assets.
4. Configure the upstream Merlin `Assets/ModInfo.asset` for your project; do not copy an upstream asset into this repository.
5. Keep Unity `.meta` files with their assets once Unity creates them.
6. Author content only in the lanes supported by the Merlin version you are using.
7. Build/export using the official Merlin workflow and validate the resulting content separately.

**Evidence state:** the overlay structure is a repository starter. It is not a claim that a particular authored asset, exported package or game build has been validated.
