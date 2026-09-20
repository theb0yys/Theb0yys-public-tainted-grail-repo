using System;
using System.Collections.Generic;
using System.Linq;

namespace AvalonExceptions;

internal static class IncidentClassifier
{
    private static readonly string[] SaveLoadRouteTokens =
    {
        "Awaken.TG.Main.Saving",
        "Awaken.TG.Main.Saving.LoadSystem",
        "Awaken.TG.Main.Saving.LoadSave",
        "Awaken.TG.MVC.Serialization.ModelTyper.CreateForDeserialization",
        "LoadSystem.Deserialize",
        "LoadSystem.LoadModelDefinitions",
        "LoadSave.LoadFromCache",
        "GameplayConstructor.RestoreGameplay",
        "SaveSlots.SaveSlot",
        "save slot has been corrupted"
    };

    internal static IncidentClassification Classify(
        string kind,
        string summary,
        string message,
        string managedStack,
        IEnumerable<string>? suspectRows,
        AddressablesCatalogCorruptFinding? addressablesCatalogFinding = null)
    {
        if (string.Equals(kind, "possible_hang_or_freeze", StringComparison.OrdinalIgnoreCase))
        {
            return new IncidentClassification(
                "possible-hang-freeze",
                "Possible loading-screen or main-thread freeze",
                "Medium",
                "The Last Menhir watchdog evidence says Unity Update stopped advancing. This supports hang/freeze triage but does not prove native crash or loading-screen state.",
                "unknown",
                "Main thread heartbeat stopped advancing; no managed exception stack was captured.",
                "Medium",
                "Treat this as hang/freeze evidence. If it happened on a loading screen, validate on a disposable run and inspect recent log or plugin changes before assigning cause.",
                "This does not prove a native crash, confirmed loading-screen state, or a specific mod cause.",
                "Record the last visible screen and recent mod changes, then reproduce in a disposable run with recent changes isolated.");
        }

        if (string.Equals(kind, "previous_session_unclean_exit", StringComparison.OrdinalIgnoreCase))
        {
            return new IncidentClassification(
                "previous-session-unclean-exit",
                "Previous session ended unexpectedly",
                "Medium",
                "The Last Menhir found a previous session marker that was not marked clean before the next launch.",
                "unknown",
                "Previous session ended before The Last Menhir could mark clean shutdown; no live managed exception stack was captured.",
                "Medium",
                "Treat this as hard-crash, force-close, or process-termination evidence. Review bounded logs, recent mod changes, and generated dependency/Harmony evidence before assigning cause.",
                "This does not prove native crash capture, the exact native fault, or a specific mod cause.",
                "Compare the previous session heartbeat time and scene with the BepInEx and Player log excerpts around the crash.");
        }

        string[] suspects = (suspectRows ?? Array.Empty<string>())
            .Where(row => !string.IsNullOrWhiteSpace(row))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        string classificationText = string.Join("\n", new[]
        {
            kind ?? string.Empty,
            summary ?? string.Empty,
            message ?? string.Empty,
            managedStack ?? string.Empty
        });
        ManagedDependencyMismatchDetection dependencyMismatch = DetectManagedDependencyMismatchRoute(classificationText);
        HarmonyTranspilerDetection harmonyTranspiler = DetectHarmonyTranspilerRoute(classificationText);
        SaveLoadRouteDetection saveLoad = DetectSaveLoadRoute(classificationText);
        LoaderDependencyRouteDetection loaderDependency = DetectLoaderDependencyRoute(kind ?? string.Empty, classificationText, suspects);
        bool addressablesInvalidPath = AddressablesInvalidPathEvidence.TryExtract(classificationText, out AddressablesInvalidPathFinding addressablesFinding);
        bool addressablesCatalogCorrupt = AddressablesCatalogCorruptEvidence.LooksLikeCatalogCorruption(classificationText);
        bool managedDependencyFollowOn = ManagedDependencyCascadeEvidence.LooksLikeEcsFollowOn(classificationText);

        if (loaderDependency.Matched)
        {
            string likelyCause = suspects.Length > 0
                ? StripEvidencePrefix(suspects[0])
                : "BepInEx reported a plugin loader dependency error.";
            return new IncidentClassification(
                "bepinex-loader-dependency-error",
                "BepInEx loader dependency error",
                loaderDependency.Confidence,
                loaderDependency.Evidence,
                suspects.Length > 0 ? "identified" : "unknown",
                likelyCause,
                BuildSuspectConfidenceLabel(suspects),
                "Install or redeploy the missing dependency for the blocked plugin. If this is a validation or probe plugin, remove or disable it when its provider mod is intentionally absent.",
                "This does not prove the blocked plugin source is broken; it may simply be installed without the provider it declares.",
                "Open bepinex_dependency_errors.csv and match dependentPluginName/dependentPluginVersion to missingDependencyGuid.");
        }

        if (addressablesInvalidPath)
        {
            return new IncidentClassification(
                "addressables-assetbundle-invalid-path",
                addressablesFinding.RouteLabel,
                "High",
                addressablesFinding.RouteEvidence,
                string.IsNullOrWhiteSpace(addressablesFinding.ModId) ? "unknown" : "identified",
                addressablesFinding.LikelyCause,
                "High",
                addressablesFinding.RecommendedAction,
                "This does not prove a native crash, BepInEx dependency mismatch, or provider framework source-code fault.",
                addressablesFinding.BestNextCheck);
        }

        if (addressablesCatalogCorrupt)
        {
            bool hasSingleCandidate = addressablesCatalogFinding.HasValue
                && !string.IsNullOrWhiteSpace(addressablesCatalogFinding.Value.ModId);
            AddressablesCatalogCorruptFinding finding = addressablesCatalogFinding.GetValueOrDefault();
            return new IncidentClassification(
                AddressablesCatalogCorruptEvidence.RouteId,
                hasSingleCandidate ? finding.RouteLabel : "Broken local mod catalog",
                "High",
                hasSingleCandidate
                    ? finding.RouteEvidence
                    : "Addressables ContentCatalogData.CreateLocator failed while loading a local mod catalog; no single catalog-bearing local mod folder was identified.",
                hasSingleCandidate ? "identified" : "unknown",
                hasSingleCandidate
                    ? finding.LikelyCause
                    : "A local mod catalog appears malformed or incompatible with Addressables, but The Last Menhir could not identify a single local mod folder from bounded runtime evidence.",
                hasSingleCandidate ? "High" : "Medium",
                hasSingleCandidate
                    ? finding.RecommendedAction
                    : "Fix, reinstall, rebuild, or remove the broken local mod folder. If multiple local mod folders contain catalog.json, isolate them one at a time before assigning cause.",
                "This does not prove a native crash, BepInEx dependency mismatch, or provider framework source-code fault.",
                hasSingleCandidate
                    ? finding.BestNextCheck
                    : "Check LocalLow/Questline/Fall of Avalon/Mods for local mod folders containing catalog.json, then rebuild or remove the malformed catalog package.");
        }

        if (managedDependencyFollowOn)
        {
            return new IncidentClassification(
                ManagedDependencyCascadeEvidence.FollowOnRouteId,
                "Follow-on noise from earlier managed dependency failure",
                "Medium",
                "Unity.Entities/SceneDisableEcsRendering null-reference evidence usually appears after an earlier managed dependency or startup load failure.",
                "unknown",
                "This looks like ECS follow-on noise from an earlier managed dependency failure, not the first fault in the startup chain.",
                "Medium",
                "Fix the earlier local-mod, loader dependency, or managed dependency/API incident first, then relaunch before investigating this ECS follow-on.",
                "This does not prove SceneDisableEcsRendering, Unity.Entities, or the game ECS layer is the root cause.",
                "Open session-summary.md and inspect the primary incident/fix-first entry before spending time on this follow-on stack.");
        }

        string routeId = "unclassified";
        string routeLabel = string.Equals(kind, "managed_exception", StringComparison.OrdinalIgnoreCase)
            ? "Unclassified managed exception"
            : "Unclassified incident";
        string routeConfidence = "Unknown";
        string routeEvidence = "No route-specific frame pattern matched in v0.1 evidence.";

        if (dependencyMismatch.Matched)
        {
            routeId = "managed-dependency-mismatch";
            routeLabel = "Managed dependency/API mismatch";
            routeConfidence = dependencyMismatch.Confidence;
            routeEvidence = dependencyMismatch.Evidence;
        }
        else if (harmonyTranspiler.Matched)
        {
            routeId = "harmony-transpiler-failure";
            routeLabel = "Harmony patch/transpiler failure";
            routeConfidence = harmonyTranspiler.Confidence;
            routeEvidence = harmonyTranspiler.Evidence;
        }
        else if (saveLoad.Matched)
        {
            routeId = "game-save-load";
            routeLabel = "Game/save load path";
            routeConfidence = saveLoad.Confidence;
            routeEvidence = saveLoad.Evidence;
        }

        if (suspects.Length > 0)
        {
            string likelyCause = suspects[0];
            string recommendedAction = "Review dependency findings and reproduce with a smaller mod list before assigning blame.";
            string doesNotProve = "This does not prove the first stack-matched plugin is the root cause; it is evidence to inspect first.";
            string bestNextCheck = "Review dependency errors, API probe rows, Harmony patch rows, and logs before disabling unrelated mods.";
            if (dependencyMismatch.Matched)
            {
                likelyCause = IsLiveDllMismatchSuspect(suspects[0])
                    ? StripEvidencePrefix(suspects[0])
                    : $"Managed dependency/API mismatch; primary suspect evidence: {suspects[0]}";
                recommendedAction = "Check that the installed/live DLL versions for the dependent mod and provider framework match. Redeploy matching builds before treating this as a source-code bug.";
                doesNotProve = "This does not prove the provider framework source is bad; it may be a stale live DLL, wrong mod pair, or consumer/provider ABI mismatch.";
                bestNextCheck = "Compare the dependent mod and provider framework versions, file timestamps, file versions, and API probe rows in the report.";
            }
            else if (harmonyTranspiler.Matched)
            {
                likelyCause = $"Harmony patch/transpiler failure; primary suspect evidence: {suspects[0]}";
                recommendedAction = "Inspect the named transpiler or patch method. If another mod also patched the same target, check Harmony re-run/reapplication behavior and static transpiler state before assuming the native method shape changed.";
                doesNotProve = "This does not prove the game method shape changed; Harmony may have rerun the transpiler after another patch was added.";
                bestNextCheck = "Inspect the patch-chain rows for the same target and confirm the transpiler is idempotent with no persistent static counter state.";
            }
            else if (saveLoad.Matched)
            {
                recommendedAction = "Treat this as a save/load-path exception with separate mod suspect evidence; validate on a disposable save before assigning cause.";
                doesNotProve = "This does not prove the save is permanently unrecoverable or that the stack-matched mod caused the save/load failure.";
                bestNextCheck = "Try a disposable copy or alternate save and compare the mod list against the last known working session.";
            }

            return new IncidentClassification(
                routeId,
                routeLabel,
                routeConfidence,
                routeEvidence,
                "identified",
                likelyCause,
                BuildSuspectConfidenceLabel(suspects),
                recommendedAction,
                doesNotProve,
                bestNextCheck);
        }

        if (dependencyMismatch.Matched)
        {
            return new IncidentClassification(
                routeId,
                routeLabel,
                routeConfidence,
                routeEvidence,
                "unknown",
                "Managed dependency/API mismatch detected; no loaded plugin assembly suspect was identified from v0.1 evidence.",
                routeConfidence,
                "Check installed/live DLL versions for recently updated mods and shared frameworks before assigning blame.",
                "This does not prove any provider framework source is bad; it may be a stale live DLL, wrong mod pair, or missing dependency.",
                "Compare installed mod versions, file timestamps, file versions, dependency-error rows, and API probe rows.");
        }

        if (harmonyTranspiler.Matched)
        {
            return new IncidentClassification(
                routeId,
                routeLabel,
                routeConfidence,
                routeEvidence,
                "unknown",
                "Harmony patch/transpiler failure detected; no loaded plugin assembly suspect was identified from v0.1 evidence.",
                routeConfidence,
                "Inspect the named transpiler or patch method and any other mods patching the same target.",
                "This does not prove the game method shape changed; Harmony patch chaining or transpiler reapplication may be enough.",
                "Inspect the captured patch method, same-target patch-chain rows, and any persistent static state in the transpiler.");
        }

        if (saveLoad.Matched)
        {
            return new IncidentClassification(
                routeId,
                routeLabel,
                routeConfidence,
                routeEvidence,
                "unknown",
                "Game/save load path detected; no mod assembly suspect was identified from v0.1 evidence.",
                routeConfidence,
                "Treat this as save/load failure evidence. Try a different save or reproduce on a disposable copy before blaming a mod.",
                "This does not prove the save is permanently unrecoverable or that a mod caused the save/load failure.",
                "Use a disposable save copy, test an alternate save, and compare the mod list against the last known working session.");
        }

        return new IncidentClassification(
            routeId,
            routeLabel,
            routeConfidence,
            routeEvidence,
            "unknown",
            "No likely cause identified from v0.1 evidence.",
            "Unknown",
            "Review dependency findings and reproduce with a smaller mod list before assigning blame.",
            "This does not prove any specific mod is at fault.",
            "Check dependency errors, API probe rows, Harmony patch rows, and recent mod changes before assigning cause.");
    }

    private static ManagedDependencyMismatchDetection DetectManagedDependencyMismatchRoute(string text)
    {
        List<string> matches = new List<string>();
        AddIfPresent(text, "ReflectionTypeLoadException", matches);
        AddIfPresent(text, "TypeLoadException", matches);
        AddIfPresent(text, "Could not load type", matches);
        AddIfPresent(text, "Could not resolve type", matches);
        AddIfPresent(text, "expected class", matches);
        AddIfPresent(text, "typeref", matches);

        bool matched = matches.Contains("ReflectionTypeLoadException", StringComparer.OrdinalIgnoreCase)
            && (matches.Contains("Could not load type", StringComparer.OrdinalIgnoreCase)
                || matches.Contains("Could not resolve type", StringComparer.OrdinalIgnoreCase)
                || matches.Contains("TypeLoadException", StringComparer.OrdinalIgnoreCase));
        if (!matched)
        {
            return ManagedDependencyMismatchDetection.None;
        }

        string confidence = matches.Count >= 4 ? "High" : "Medium";
        return new ManagedDependencyMismatchDetection(true, confidence, string.Join("; ", matches.Distinct(StringComparer.OrdinalIgnoreCase)));
    }

    private static HarmonyTranspilerDetection DetectHarmonyTranspilerRoute(string text)
    {
        List<string> matches = new List<string>();
        AddIfPresent(text, "Harmony", matches);
        AddIfPresent(text, "HarmonyLib", matches);
        AddIfPresent(text, "Transpile", matches);
        AddIfPresent(text, "transpiler", matches);
        AddIfPresent(text, "CodeInstruction", matches);
        AddIfPresent(text, "PatchProcessor", matches);

        bool hasTranspilerEvidence = matches.Contains("Transpile", StringComparer.OrdinalIgnoreCase)
            || matches.Contains("transpiler", StringComparer.OrdinalIgnoreCase);
        bool hasPatchEvidence = matches.Contains("Harmony", StringComparer.OrdinalIgnoreCase)
            || matches.Contains("HarmonyLib", StringComparer.OrdinalIgnoreCase)
            || matches.Contains("CodeInstruction", StringComparer.OrdinalIgnoreCase)
            || matches.Contains("PatchProcessor", StringComparer.OrdinalIgnoreCase);
        if (!hasTranspilerEvidence || !hasPatchEvidence)
        {
            return HarmonyTranspilerDetection.None;
        }

        string confidence = matches.Count >= 3 ? "High" : "Medium";
        return new HarmonyTranspilerDetection(true, confidence, string.Join("; ", matches.Distinct(StringComparer.OrdinalIgnoreCase)));
    }

    private static SaveLoadRouteDetection DetectSaveLoadRoute(string text)
    {
        List<string> matches = new List<string>();
        foreach (string token in SaveLoadRouteTokens)
        {
            if (text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                matches.Add(token);
            }
        }

        bool explicitSaveSlot = text.IndexOf("save slot", StringComparison.OrdinalIgnoreCase) >= 0;
        bool matched = matches.Count > 0 || explicitSaveSlot;
        if (!matched)
        {
            return SaveLoadRouteDetection.None;
        }

        if (explicitSaveSlot && matches.All(token => !token.Equals("save slot has been corrupted", StringComparison.OrdinalIgnoreCase)))
        {
            matches.Add("save slot");
        }

        string confidence = matches.Count >= 2 || matches.Contains("save slot has been corrupted", StringComparer.OrdinalIgnoreCase)
            ? "High"
            : "Medium";

        return new SaveLoadRouteDetection(true, confidence, string.Join("; ", matches.Distinct(StringComparer.OrdinalIgnoreCase)));
    }

    private static LoaderDependencyRouteDetection DetectLoaderDependencyRoute(string kind, string text, string[] suspects)
    {
        bool loaderKind = string.Equals(kind, "bepinex_loader_dependency_error", StringComparison.OrdinalIgnoreCase);
        bool hasLoaderText = text.IndexOf("BepInEx loader dependency error", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Chainloader.DependencyErrors", StringComparison.OrdinalIgnoreCase) >= 0
            || (text.IndexOf("Could not load [", StringComparison.OrdinalIgnoreCase) >= 0
                && text.IndexOf("missing dependencies", StringComparison.OrdinalIgnoreCase) >= 0);
        bool hasLoaderSuspect = suspects.Any(suspect => suspect.IndexOf("BepInEx loader dependency error", StringComparison.OrdinalIgnoreCase) >= 0);
        if (!loaderKind && !hasLoaderText && !hasLoaderSuspect)
        {
            return LoaderDependencyRouteDetection.None;
        }

        string confidence = loaderKind || hasLoaderSuspect ? "High" : "Medium";
        return new LoaderDependencyRouteDetection(
            true,
            confidence,
            "BepInEx Chainloader dependency evidence identified a plugin that could not load because a declared dependency was missing.");
    }

    private static bool IsLiveDllMismatchSuspect(string suspect)
    {
        return suspect.IndexOf("Live DLL mismatch", StringComparison.OrdinalIgnoreCase) >= 0
            && suspect.IndexOf("live DLLs do not match", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string StripEvidencePrefix(string suspect)
    {
        const string HighConfidence = " - high confidence: ";
        int marker = suspect.IndexOf(HighConfidence, StringComparison.OrdinalIgnoreCase);
        if (marker >= 0)
        {
            return suspect.Substring(marker + HighConfidence.Length).Trim();
        }

        int colon = suspect.IndexOf(':');
        return colon >= 0 && colon + 1 < suspect.Length
            ? suspect.Substring(colon + 1).Trim()
            : suspect.Trim();
    }

    private static void AddIfPresent(string text, string token, List<string> matches)
    {
        if (text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            matches.Add(token);
        }
    }

    private static string BuildSuspectConfidenceLabel(IEnumerable<string> suspects)
    {
        string[] rows = suspects.ToArray();
        if (rows.Any(item => item.IndexOf("high confidence", StringComparison.OrdinalIgnoreCase) >= 0))
        {
            return "High";
        }

        if (rows.Any(item => item.IndexOf("medium confidence", StringComparison.OrdinalIgnoreCase) >= 0))
        {
            return "Medium";
        }

        if (rows.Any(item => item.IndexOf("low confidence", StringComparison.OrdinalIgnoreCase) >= 0))
        {
            return "Low";
        }

        return "Unknown";
    }

    private readonly struct SaveLoadRouteDetection
    {
        internal static readonly SaveLoadRouteDetection None = new SaveLoadRouteDetection(false, "Unknown", string.Empty);

        internal SaveLoadRouteDetection(bool matched, string confidence, string evidence)
        {
            Matched = matched;
            Confidence = confidence;
            Evidence = evidence;
        }

        internal bool Matched { get; }
        internal string Confidence { get; }
        internal string Evidence { get; }
    }

    private readonly struct ManagedDependencyMismatchDetection
    {
        internal static readonly ManagedDependencyMismatchDetection None = new ManagedDependencyMismatchDetection(false, "Unknown", string.Empty);

        internal ManagedDependencyMismatchDetection(bool matched, string confidence, string evidence)
        {
            Matched = matched;
            Confidence = confidence;
            Evidence = evidence;
        }

        internal bool Matched { get; }
        internal string Confidence { get; }
        internal string Evidence { get; }
    }

    private readonly struct HarmonyTranspilerDetection
    {
        internal static readonly HarmonyTranspilerDetection None = new HarmonyTranspilerDetection(false, "Unknown", string.Empty);

        internal HarmonyTranspilerDetection(bool matched, string confidence, string evidence)
        {
            Matched = matched;
            Confidence = confidence;
            Evidence = evidence;
        }

        internal bool Matched { get; }
        internal string Confidence { get; }
        internal string Evidence { get; }
    }

    private readonly struct LoaderDependencyRouteDetection
    {
        internal static readonly LoaderDependencyRouteDetection None = new LoaderDependencyRouteDetection(false, "Unknown", string.Empty);

        internal LoaderDependencyRouteDetection(bool matched, string confidence, string evidence)
        {
            Matched = matched;
            Confidence = confidence;
            Evidence = evidence;
        }

        internal bool Matched { get; }
        internal string Confidence { get; }
        internal string Evidence { get; }
    }
}

internal readonly struct IncidentClassification
{
    internal IncidentClassification(
        string routeId,
        string routeLabel,
        string routeConfidence,
        string routeEvidence,
        string modSuspectStatus,
        string likelyCauseLabel,
        string confidenceLabel,
        string recommendedAction,
        string doesNotProve,
        string bestNextCheck)
    {
        RouteId = routeId;
        RouteLabel = routeLabel;
        RouteConfidence = routeConfidence;
        RouteEvidence = routeEvidence;
        ModSuspectStatus = modSuspectStatus;
        LikelyCauseLabel = likelyCauseLabel;
        ConfidenceLabel = confidenceLabel;
        RecommendedAction = recommendedAction;
        DoesNotProve = doesNotProve;
        BestNextCheck = bestNextCheck;
    }

    internal string RouteId { get; }
    internal string RouteLabel { get; }
    internal string RouteConfidence { get; }
    internal string RouteEvidence { get; }
    internal string ModSuspectStatus { get; }
    internal string LikelyCauseLabel { get; }
    internal string ConfidenceLabel { get; }
    internal string RecommendedAction { get; }
    internal string DoesNotProve { get; }
    internal string BestNextCheck { get; }
}
