# Recipe: Optional Infrastructure Bridge

Use this when the base mod must work without the optional infrastructure.

If the infrastructure is required for the advertised feature, use a hard dependency instead.

## Pattern

Keep all optional API access in one bridge class:

```csharp
using System;
using System.Reflection;

internal static class OptionalModManagerBridge
{
    private const string ApiTypeName =
        "FoAModManager.FoAModManagerApi, FoAModManager";

    private static readonly Type? ApiType =
        Type.GetType(ApiTypeName, throwOnError: false);

    internal static bool Available => ApiType != null;

    internal static bool TrySetCustomUiScope(
        string ownerId,
        bool active,
        bool freezeWorld)
    {
        MethodInfo? method = ApiType?.GetMethod(
            "SetCustomUiScope",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new[] {
                typeof(string),
                typeof(bool),
                typeof(bool)
            },
            modifiers: null);

        if (method == null)
        {
            return false;
        }

        try
        {
            method.Invoke(null, new object[] {
                ownerId,
                active,
                freezeWorld
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

## Fail closed

Optional integration failure should disable only the enhancement.

Bad:

```text
Mod Manager missing
→ custom feature state corrupts / plugin crashes
```

Good:

```text
Mod Manager missing
→ manager-specific integration disabled
→ base mod remains valid
```

## Keep the reflection surface narrow

Do not build a generic “call anything by string” layer.

For each bridge:

- exact assembly-qualified API type;
- exact public method name/signature;
- explicit exception containment;
- no private-member fallback;
- no similarly named replacement search.

## Optional Tainted Interface

The same principle applies to optional visual enhancement:

```text
Tainted Interface present
→ use semantic shared styles/assets

absent
→ use your reviewed local fallback
```

If your screen cannot render/function correctly without Tainted Interface, make it a hard dependency instead.
