# Weapons

Document type: **domain system hub**.

Weapon work crosses several owners. Read this page first to keep item identity, equip lifecycle, combat and presentation separate.

## End-to-end map

```text
item/template identity
→ acquisition
→ native equip lifecycle
→ native combat owner
→ presentation owner
→ cleanup
→ separate persistence/compatibility questions
```

## Canonical chapters

### Understand the native system

- [Native weapon lifecycle](native-lifecycle.md)
- [Items and Inventory](../items/README.md)

### Perform the bounded mechanic

- [Custom weapon integration](../../mechanics/weapons/custom-weapon-integration.md)

### Learn in sequence

- [Weapons content-authoring journey](../../learn/content-authoring/weapons/README.md)

## Diagnostic principle

If a weapon exists and functions in combat but renders incorrectly, treat **presentation** as the first failed lane. Do not reopen item registration, damage or animation without evidence.

## Proof boundary

The domain model is stronger than the current generic importer completion state. Persistence, transactional registration, hot-unload and broad compatibility remain separate/incomplete unless explicitly proven.
