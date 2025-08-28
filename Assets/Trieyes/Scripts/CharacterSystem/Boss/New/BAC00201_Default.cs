using AttackComponents;
using AttackSystem;
using CharacterSystem;
using UnityEngine;

public class BAC00201_Default : AttackComponent
{
    [SerializeField] private float speed = 6;
    
    public override void Activate(Attack attack, Vector2 direction)
    {
        base.Activate(attack, direction);
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    protected override void Update()
    {
        base.Update();

        attack.transform.position += attack.transform.right * (speed * Time.deltaTime);
    }

    public override void ProcessComponentCollision(Pawn targetPawn)
    {
        AttackFactory.Instance.Deactivate(attack);
    }
}