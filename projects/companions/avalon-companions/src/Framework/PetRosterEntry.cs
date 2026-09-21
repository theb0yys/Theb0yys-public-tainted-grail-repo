namespace AvalonCompanions.Framework;

internal readonly struct PetRosterEntry
{
    internal PetRosterEntry(
        string displayName,
        string templateName,
        string guid,
        bool runtimeApproved,
        bool requiresPetComponent,
        float placementRight,
        float placementBack,
        string reviewId,
        string blockReason)
    {
        DisplayName = displayName;
        TemplateName = templateName;
        Guid = guid;
        RuntimeApproved = runtimeApproved;
        RequiresPetComponent = requiresPetComponent;
        PlacementRight = placementRight;
        PlacementBack = placementBack;
        ReviewId = reviewId;
        BlockReason = blockReason;
    }

    internal string DisplayName { get; }

    internal string TemplateName { get; }

    internal string Guid { get; }

    internal bool RuntimeApproved { get; }

    internal bool RequiresPetComponent { get; }

    internal float PlacementRight { get; }

    internal float PlacementBack { get; }

    internal string ReviewId { get; }

    internal string BlockReason { get; }
}
