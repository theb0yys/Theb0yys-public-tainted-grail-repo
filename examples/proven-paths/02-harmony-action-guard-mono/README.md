# 02 — Harmony Action Guard

Use this pattern when a native/original action should run normally **unless a specific guard blocks it**.

The demo target belongs to this template. No FoA action is patched.

## Mechanism

\`\`\`text
action requested
      ↓
prefix checks guard
      ├── allowed → return true → original action runs
      └── blocked → set a valid blocked result → return false
\`\`\`

The key idea is that your mod does not become the whole action system. It only owns the guard.

## Build

\`\`\`powershell
dotnet build .\ActionGuardTemplate.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
\`\`\`

With \`Demo.AllowAction=false\`, the demo should log a blocked result and the original execution count should stay at zero.

With it true, the original action runs.

## Adapting it

Before replacing \`DemoAction.TryExecute()\`:

- verify the exact target and return type;
- understand what result the original caller expects when an action is rejected;
- skip the original only for the condition you actually own;
- preserve the normal native path when allowed;
- do not invent state cleanup—research what the target action owns.

