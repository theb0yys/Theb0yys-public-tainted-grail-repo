using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Interactions;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Actions;

namespace AvalonHumanCompanions;

internal enum NativeHumanCompanionCommand
{
    Follow,
    Hold,
    Defend,
    ComeClose,
    Recall,
    Dismiss
}

internal sealed class HumanCompanionCommandAction : AbstractLocationAction
{
    public override bool IsNotSaved => true;

    public override string DefaultActionName => "Companion";

    protected override InteractRunType RunInteraction => InteractRunType.DontRun;

    protected override InfoFrame ActionFrameInternal => new InfoFrame(DefaultActionName, isButtonActive: true);

    public override ActionAvailability GetAvailability(Hero hero, IInteractableWithHero interactable)
    {
        Location location = interactable as Location ?? ParentModel;
        return Plugin.IsNativeHumanCommandMenuAvailable(location, hero, interactable)
            ? ActionAvailability.Available
            : ActionAvailability.Disabled;
    }

    protected override void OnStart(Hero hero, IInteractableWithHero interactable)
    {
        Location location = interactable as Location ?? ParentModel;
        Plugin.OpenNativeHumanCommandMenu(location);
    }
}

internal abstract class HumanCompanionQuickCommandAction : AbstractLocationAction
{
    public override bool IsNotSaved => true;

    public override string DefaultActionName => ActionLabel;

    protected abstract string ActionLabel { get; }

    protected abstract NativeHumanCompanionCommand Command { get; }

    protected override InteractRunType RunInteraction => InteractRunType.DontRun;

    protected override InfoFrame ActionFrameInternal => new InfoFrame(ActionLabel, isButtonActive: true);

    public override ActionAvailability GetAvailability(Hero hero, IInteractableWithHero interactable)
    {
        Location location = interactable as Location ?? ParentModel;
        return Plugin.IsNativeHumanCommandAvailable(location, hero, interactable, Command)
            ? ActionAvailability.Available
            : ActionAvailability.Disabled;
    }

    protected override void OnStart(Hero hero, IInteractableWithHero interactable)
    {
        Location location = interactable as Location ?? ParentModel;
        Plugin.RunNativeHumanCommand(location, Command);
    }
}

internal sealed class HumanCompanionFollowAction : HumanCompanionQuickCommandAction
{
    protected override string ActionLabel => "Follow";

    protected override NativeHumanCompanionCommand Command => NativeHumanCompanionCommand.Follow;
}

internal sealed class HumanCompanionDefendAction : HumanCompanionQuickCommandAction
{
    protected override string ActionLabel => "Defend";

    protected override NativeHumanCompanionCommand Command => NativeHumanCompanionCommand.Defend;
}

internal sealed class HumanCompanionHoldAction : HumanCompanionQuickCommandAction
{
    protected override string ActionLabel => "Hold";

    protected override NativeHumanCompanionCommand Command => NativeHumanCompanionCommand.Hold;
}

internal sealed class HumanCompanionComeCloseAction : HumanCompanionQuickCommandAction
{
    protected override string ActionLabel => "Come Close";

    protected override NativeHumanCompanionCommand Command => NativeHumanCompanionCommand.ComeClose;
}

internal sealed class HumanCompanionRecallAction : HumanCompanionQuickCommandAction
{
    protected override string ActionLabel => "Recall";

    protected override NativeHumanCompanionCommand Command => NativeHumanCompanionCommand.Recall;
}

internal sealed class HumanCompanionDismissAction : HumanCompanionQuickCommandAction
{
    protected override string ActionLabel => "Dismiss";

    protected override NativeHumanCompanionCommand Command => NativeHumanCompanionCommand.Dismiss;
}
