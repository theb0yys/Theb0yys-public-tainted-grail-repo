# Creature Pipeline Failure Catalogue

## Visual root loads but actor construction fails

The visual-transport stage completed, but actor construction still needs diagnosis. Check template contract, component order, controller data, and resolver/provider identity.

## Actor appears but drags or will not move correctly

Check controller/grounding data, root-motion assumptions, and Movement animation mapping. Collider presence alone is insufficient.

## Actor attacks but death is broken

Combat proof did not prove death. Check GetHit/Death mappings, native lethal transition, and corpse handoff.

## Actor dies and body disappears immediately

Check cleanup timing. Do not discard the whole Location before the native NpcDummy/Corpse handoff completes.

## Generic HDRP/Lit or visible mesh looks wrong under Kandra

Visibility is not native character-renderer compatibility. Re-open the native renderer/material/Kandra baseline instead of treating a generic shader as proof.

## Headless asset test passes but visual runtime fails

A non-graphics environment cannot stand in for Kandra/HDRP visual acceptance.

## One creature works but another fails

Do not generalise the native baseline. Re-run CI1–CI5 for the new creature family.

## Controlled actor works but ambient spawns fail

Single-actor runtime proof is not population ownership. Re-open the placement/population stage.

## Actor remains after restart unexpectedly

Verify the save policy/cleanup lane, including MarkedNotSaved or the intended durable persistence contract and duplicate prevention.
