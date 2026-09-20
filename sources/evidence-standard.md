# Public Evidence Standard

Reader-facing proof labels:

- **Official**
- **Static/source inspected**
- **Build validated**
- **Loader validated**
- **Runtime proven**
- **Persistence proven**
- **Compatibility tested**
- **Release validated**
- **Unknown / not tested**

Rules:

1. One proof lane does not silently substitute for another.
2. Static/decompiled evidence does not prove runtime success.
3. Runtime success does not prove persistence safety.
4. One tested build does not prove general compatibility.
5. A build pass does not prove the packaged release.
6. State exact game/runtime/dependency scope for patch-sensitive claims.
7. Update the canonical working route instead of retaining obsolete parallel instructions.
