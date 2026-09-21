# Tainted Armour Unity Inspection

This folder owns the Unity-facing validation transport for the standalone
Tainted Armour importer. `Tainted.Armour.Unity.EditorValidation.csproj` builds
the editor command; the command uses the production
`Tainted.Armour.Unity.UnityGeometrySnapshotAdapter` and invokes
`Tainted.Armour.ArmorImporter.Import` for every real sample row.

The command is validation-only. It reads an already imported source FBX and a
native animation clip, mutates only a transient instantiated object, restores
that transient state, and emits a typed JSON receipt. It does not apply a
candidate map, mutate the FBX, convert payloads, generate sidecars, change a
runtime loader, register or equip an item, write inventory/save/native game
state, build an AssetBundle or Addressables content, or claim release readiness.
It also does not enter a live FoA body/equip scene or fabricate a
`VisualRuntimeObservationRequest`, so the A2K-T34 visual-runtime stage remains
not requested here rather than accepted. Live A2K-T34 observations belong to the
separate diagnostic-tool transport.

When `-taintedArmourFullGeometryCaptureRoot` is supplied, the command also
writes the importer-owned baked position blob and evaluated submesh-index blob
beside a `capture.geometry.json` receipt. Those capture files are still
validation artifacts: candidate-map application, conversion, and downstream
writes remain false.

The current real-sample route is the six T56 fall/Air_Loop rows. Their accepted
observation is 444 orientation-reversed triangle records and zero collapsed
records; this is a successful execution of the importer but a failed
zero-inversion deformation gate and not in-game visual proof.
