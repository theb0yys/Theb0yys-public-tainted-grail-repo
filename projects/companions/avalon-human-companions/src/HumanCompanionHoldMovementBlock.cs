using Awaken.TG.MVC.Elements;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Fights.NPCs.Providers;

namespace AvalonHumanCompanions;

internal sealed class HumanCompanionHoldMovementBlock : Element<NpcElement>, ICanMoveProvider
{
    public override bool IsNotSaved => true;

    public bool CanMove => false;

    public bool CanOverrideDestination => false;

    public bool ResetMovementSpeed => true;

    protected override void OnInitialize()
    {
        NpcCanMoveHandler.AddCanMoveProvider(ParentModel, this);
    }

    protected override void OnDiscard(bool fromDomainDrop)
    {
        if (ParentModel != null && !ParentModel.HasBeenDiscarded)
        {
            NpcCanMoveHandler.RemoveCanMoveProvider(ParentModel, this);
        }
    }
}
