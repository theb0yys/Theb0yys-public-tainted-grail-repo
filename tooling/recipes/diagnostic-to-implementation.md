# Recipe: Diagnostic Evidence to Implementation

```text
question
→ collect read-only diagnostic dump
→ identify exact GUID/type/owner/context
→ record evidence and unknowns
→ inspect native owner/lifecycle
→ choose smallest mechanic
→ implement
→ runtime validation
→ persistence/compatibility validation when relevant
```

Do not skip from “CSV row exists” directly to mutation.

The Diagnostic Tool answers **what exists / what was loaded**. The system/mechanic research answers **what safely owns the behaviour**.
