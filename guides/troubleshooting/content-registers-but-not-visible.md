---
document_type: troubleshooting
scope: registered/equipped content is not visible
runtime: mono
last_verified: 2026-09-20
---

# Content Registers but Is Not Visible

Separate content identity from presentation.

A successful template lookup, inventory grant or equip event does not prove rendering.

For weapons, trace:

```text
custom ItemTemplate
→ Item / ItemEquip
→ native CharacterHandBase presentation
→ DrakeLodGroup / DrakeMeshRenderer
→ mesh/material key resolution
→ renderer entity / visibility
```

If combat and inventory work but the object is invisible, keep registration/combat closed unless evidence shows they failed. Investigate the presentation owner.

See [Equipped weapon presentation](../../knowledge/mechanics/weapons/equipped-presentation.md).
