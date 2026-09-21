using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Interactions;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Actions;
using AvalonCompanions.Framework;

namespace AvalonCompanions.Patches;

internal sealed class AvalonCompanionCommandAction : AbstractLocationAction
{
    private const string ActionLabel = "Companion";

    public override bool IsNotSaved => true;

    public override string DefaultActionName => ActionLabel;

    protected override InteractRunType RunInteraction => InteractRunType.DontRun;

    protected override InfoFrame ActionFrameInternal => new InfoFrame(ActionLabel, isButtonActive: true);

    public override ActionAvailability GetAvailability(Hero hero, IInteractableWithHero interactable)
    {
        return PetCompanionController.IsNativeCompanionCommandAvailable(ParentModel, hero, interactable)
            ? ActionAvailability.Available
            : ActionAvailability.Disabled;
    }

    protected override void OnStart(Hero hero, IInteractableWithHero interactable)
    {
        Location location = interactable as Location ?? ParentModel;
        PetCompanionController.OpenNativeCompanionCommand(location);
    }
}

internal abstract class AvalonCompanionQuickCommandAction : AbstractLocationAction
{
    public override bool IsNotSaved => true;

    public override string DefaultActionName => ActionLabel;

    protected abstract string ActionLabel { get; }

    protected abstract NativeCompanionCommand Command { get; }

    protected override InteractRunType RunInteraction => InteractRunType.DontRun;

    protected override InfoFrame ActionFrameInternal => new InfoFrame(ActionLabel, isButtonActive: true);

    public override ActionAvailability GetAvailability(Hero hero, IInteractableWithHero interactable)
    {
        Location location = interactable as Location ?? ParentModel;
        return PetCompanionController.IsNativeCompanionQuickCommandAvailable(location, hero, interactable, Command)
            ? ActionAvailability.Available
            : ActionAvailability.Disabled;
    }

    protected override void OnStart(Hero hero, IInteractableWithHero interactable)
    {
        Location location = interactable as Location ?? ParentModel;
        PetCompanionController.RunNativeCompanionQuickCommand(location, Command);
    }
}

internal sealed class AvalonCompanionFollowAction : AvalonCompanionQuickCommandAction
{
    protected override string ActionLabel => "Follow";

    protected override NativeCompanionCommand Command => NativeCompanionCommand.Follow;
}

internal sealed class AvalonCompanionStayAction : AvalonCompanionQuickCommandAction
{
    protected override string ActionLabel => "Hold Position";

    protected override NativeCompanionCommand Command => NativeCompanionCommand.Stay;
}

internal sealed class AvalonCompanionDefendAction : AvalonCompanionQuickCommandAction
{
    protected override string ActionLabel => "Defend";

    protected override NativeCompanionCommand Command => NativeCompanionCommand.Defend;
}

internal sealed class AvalonCompanionComeCloseAction : AvalonCompanionQuickCommandAction
{
    protected override string ActionLabel => "Come Close";

    protected override NativeCompanionCommand Command => NativeCompanionCommand.ComeClose;
}

internal sealed class AvalonCompanionRecallAction : AvalonCompanionQuickCommandAction
{
    protected override string ActionLabel => "Recall";

    protected override NativeCompanionCommand Command => NativeCompanionCommand.Recall;
}

internal sealed class AvalonCompanionDismissAction : AvalonCompanionQuickCommandAction
{
    protected override string ActionLabel => "Dismiss";

    protected override NativeCompanionCommand Command => NativeCompanionCommand.Dismiss;
}

internal sealed class AvalonCompanionRecoverAction : AvalonCompanionQuickCommandAction
{
    protected override string ActionLabel => "Recover";

    protected override NativeCompanionCommand Command => NativeCompanionCommand.Recover;
}
