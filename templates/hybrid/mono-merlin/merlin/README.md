# Merlin Side

Use the official Merlin Workshop project and the repository's [Merlin basic overlay](../../../merlin/basic/).

For this hybrid starter, rename the overlay's content root to your mod identity and use the stable identifier recorded in `../contract/README.md` anywhere an explicit authored identifier is needed.

Do not add a project reference from the Merlin Unity project to the BepInEx runtime project merely to share constants. Keep the build lifecycles independent; duplicate the small stable identifier intentionally and review it when either side changes.
