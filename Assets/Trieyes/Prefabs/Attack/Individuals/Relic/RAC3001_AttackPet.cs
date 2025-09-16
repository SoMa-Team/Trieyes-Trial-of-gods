using AttackComponents;
using AttackSystem;
using CharacterSystem;
using UnityEngine;

public class RAC3001_AttackPet : AttackComponent
{
    private Pawn target;
    
    public override void Activate(Attack attack, Vector2 direction)
    {
        base.Activate(attack, direction);
        target = attack.attacker;
    }

    public override void Deactivate()
    {
        target = null;
        base.Deactivate();
    }

    protected override void Update()
    {
        base.Update();
        
        
    }

    public override void ProcessComponentCollision(Pawn targetPawn)
    {
        base.ProcessComponentCollision(targetPawn);
    }
}
