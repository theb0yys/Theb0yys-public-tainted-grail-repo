using System;

namespace AvalonCompanions.Framework;

internal sealed class CompanionCoreRuntimeProfile
{
    internal const int CurrentContractVersion = 2;

    internal CompanionCoreRuntimeProfile(
        PetRosterEntry entry,
        CompanionRuntimeCombatRole combatRole,
        CompanionRuntimeTemperament temperament,
        CompanionRuntimeCommandCapabilities commandCapabilities,
        int profileSeed,
        int createdFrame)
    {
        ContractVersion = CurrentContractVersion;
        TemplateGuid = entry.Guid;
        TemplateName = entry.TemplateName;
        DisplayName = entry.DisplayName;
        ReviewId = entry.ReviewId;
        RequiresPetComponent = entry.RequiresPetComponent;
        RuntimeApproved = entry.RuntimeApproved;
        CombatRole = combatRole;
        Temperament = temperament;
        CommandCapabilities = commandCapabilities;
        ProfileSeed = profileSeed;
        RuntimeState = CompanionRuntimeProfileState.Known;
        TrustProfile = new CompanionTrustProfile(createdFrame);
        LastCommand = "created";
        LastCommandReason = "runtime profile created";
        CreatedFrame = createdFrame;
        LastSeenFrame = createdFrame;
        LastCommandFrame = createdFrame;
    }

    internal int ContractVersion { get; }

    internal string TemplateGuid { get; }

    internal string TemplateName { get; }

    internal string DisplayName { get; }

    internal string ReviewId { get; }

    internal bool RequiresPetComponent { get; }

    internal bool RuntimeApproved { get; }

    internal CompanionRuntimeCombatRole CombatRole { get; }

    internal CompanionRuntimeTemperament Temperament { get; }

    internal CompanionRuntimeCommandCapabilities CommandCapabilities { get; }

    internal int ProfileSeed { get; }

    internal CompanionRuntimeProfileState RuntimeState { get; set; }

    internal CompanionTrustProfile TrustProfile { get; }

    internal CompanionMode LastKnownMode { get; set; }

    internal FollowRangeProfile LastKnownFollowRange { get; set; }

    internal string LastCommand { get; set; }

    internal string LastCommandReason { get; set; }

    internal int CreatedFrame { get; }

    internal int LastSeenFrame { get; set; }

    internal int LastCommandFrame { get; set; }

    internal int ActivationCount { get; set; }
}

internal enum CompanionRuntimeCombatRole
{
    NativePet,
    PassiveCompanion,
    OffensiveAlly,
    UndeadAlly,
}

internal enum CompanionRuntimeTemperament
{
    Steady,
    Skittish,
    Pack,
    Fierce,
    Volatile,
    Bound,
}

internal enum CompanionRuntimeProfileState
{
    Known,
    Selected,
    Active,
    Dismissed,
    Removed,
}

[Flags]
internal enum CompanionRuntimeCommandCapabilities
{
    None = 0,
    Follow = 1 << 0,
    Stay = 1 << 1,
    Range = 1 << 2,
    Recall = 1 << 3,
    Recover = 1 << 4,
    Dismiss = 1 << 5,
    NativeDefendPrompt = 1 << 6,
}
