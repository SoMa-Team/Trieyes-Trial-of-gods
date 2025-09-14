using AttackComponents;
using AttackSystem;
using CharacterSystem;
using UnityEngine;

public class RAC3002_GuidedMissile : AttackComponent
{
    public override void Activate(Attack attack, Vector2 direction)
    {
        base.Activate(attack, direction);
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    public override void ProcessComponentCollision(Pawn targetPawn)
    {
        base.ProcessComponentCollision(targetPawn);
    }
}
