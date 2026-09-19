# Serialization and Archives

Do not infer a file format from its extension alone.

FoA content can pass through Unity containers, game-specific serialization and runtime registries. Treat those layers separately:

~~~text
package/container
→ serialized game data
→ native loader/registry
→ runtime owner
~~~

## Rule

Use the game's current loader/registry for the content type you are modifying. Do not build a custom archive writer merely because an installed file can be read or inspected.

For runtime template ownership, see [Templates and Registries](TEMPLATES_REGISTRIES.md).
