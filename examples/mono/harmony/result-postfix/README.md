# Harmony Result Postfix

Use this pattern when the original method should still run and you only need to adjust its final decision/result.

This template patches a **self-owned** demo method. It does not target Tainted Grail.

## Mechanism

\`\`\`text
original method runs
        ↓
original result exists
        ↓
postfix inspects config/context
        ↓
postfix changes only the result when required
\`\`\`

That is substantially safer than replacing an entire method when all you need is a changed boolean/nullable decision.

## Build

\`\`\`powershell
dotnet build .\HarmonyResultPostfix.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
\`\`\`

Optional local deploy:

\`\`\`powershell
dotnet build .\HarmonyResultPostfix.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA" -p:DeployOnBuild=true
\`\`\`

## Expected demo log

With \`Demo.ForceAllow=true\`:

\`\`\`text
Result Postfix Template demo: original=false final=true
\`\`\`

## Turning this into a game mod

Replace \`DemoDecision.ShouldAllow()\` only after you have verified the exact current game type/method.

Preserve these invariants:

- original execution stays intact;
- your postfix is narrow;
- config/context can disable the override;
- if the target cannot be found, fail closed and log it;
- after a game update, reverify the target.

