using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Interactions;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Actions;

namespace AvalonBroodmotherCompanion;

internal sealed class BroodmotherCompanionCommandAction : AbstractLocationAction
{
    private const string ActionLabel = "Companion";

    public override bool IsNotSaved => true;

    public override string DefaultActionName => ActionLabel;

    protected override InteractRunType RunInteraction => InteractRunType.DontRun;

    protected override InfoFrame ActionFrameInternal => new InfoFrame(ActionLabel, isButtonActive: true);

    public override ActionAvailability GetAvailability(Hero hero, IInteractableWithHero interactable)
    {
        return Plugin.IsNativeBroodmotherCommandMenuAvailable(ParentModel, hero, interactable)
            ? ActionAvailability.Available
            : ActionAvailability.Disabled;
    }

    protected override void OnStart(Hero hero, IInteractableWithHero interactable)
    {
        Location location = interactable as Location ?? ParentModel;
        Plugin.OpenNativeBroodmotherCommandMenu(location);
    }
}

internal abstract class BroodmotherCompanionQuickCommandAction : AbstractLocationAction
{
    public override bool IsNotSaved => true;

    public override string DefaultActionName => ActionLabel;

    protected abstract string ActionLabel { get; }

    protected abstract BroodmotherDialogueCommand Command { get; }

    protected override InteractRunType RunInteraction => InteractRunType.DontRun;

    protected override InfoFrame ActionFrameInternal => new InfoFrame(ActionLabel, isButtonActive: true);

    public override ActionAvailability GetAvailability(Hero hero, IInteractableWithHero interactable)
    {
        Location location = interactable as Location ?? ParentModel;
        return Plugin.IsNativeBroodmotherQuickCommandAvailable(location, hero, interactable, Command)
            ? ActionAvailability.Available
            : ActionAvailability.Disabled;
    }

    protected override void OnStart(Hero hero, IInteractableWithHero interactable)
    {
        Location location = interactable as Location ?? ParentModel;
        Plugin.RunNativeBroodmotherQuickCommand(location, Command);
    }
}

internal sealed class BroodmotherCompanionFollowAction : BroodmotherCompanionQuickCommandAction
{
    protected override string ActionLabel => "Follow";

    protected override BroodmotherDialogueCommand Command => BroodmotherDialogueCommand.Follow;
}

internal sealed class BroodmotherCompanionHoldPositionAction : BroodmotherCompanionQuickCommandAction
{
    protected override string ActionLabel => "Hold Position";

    protected override BroodmotherDialogueCommand Command => BroodmotherDialogueCommand.HoldPosition;
}

internal sealed class BroodmotherCompanionDefendAction : BroodmotherCompanionQuickCommandAction
{
    protected override string ActionLabel => "Defend";

    protected override BroodmotherDialogueCommand Command => BroodmotherDialogueCommand.Defend;
}

internal sealed class BroodmotherCompanionComeCloseAction : BroodmotherCompanionQuickCommandAction
{
    protected override string ActionLabel => "Come Close";

    protected override BroodmotherDialogueCommand Command => BroodmotherDialogueCommand.ComeClose;
}

internal sealed class BroodmotherCompanionRecallAction : BroodmotherCompanionQuickCommandAction
{
    protected override string ActionLabel => "Recall";

    protected override BroodmotherDialogueCommand Command => BroodmotherDialogueCommand.Recall;
}

internal sealed class BroodmotherCompanionRecoverAction : BroodmotherCompanionQuickCommandAction
{
    protected override string ActionLabel => "Recover";

    protected override BroodmotherDialogueCommand Command => BroodmotherDialogueCommand.Recover;
}

internal sealed class BroodmotherCompanionDismissAction : BroodmotherCompanionQuickCommandAction
{
    protected override string ActionLabel => "Dismiss";

    protected override BroodmotherDialogueCommand Command => BroodmotherDialogueCommand.Dismiss;
}
