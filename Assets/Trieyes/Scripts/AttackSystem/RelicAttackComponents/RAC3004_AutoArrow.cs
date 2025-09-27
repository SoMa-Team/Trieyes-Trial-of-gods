using AttackComponents;
using AttackSystem;
using BattleSystem;
using CharacterSystem;
using UnityEngine;

public class RAC3004_AutoArrow : AttackComponent
{
    private enum AttackState
    {
        Ready,
        Linear,
        Target,
    }

    private const float linearDuration = 1f;
    private const float speed = 3f;
    private const float anchorDelta = 2f;
    private const float targetDuration = 1f;
    
    [SerializeField] private AttackData selfAttackData;
    
    private AttackState state;
    private float targetStartTime;
    private Pawn target;
    private Vector3 startPosition;
    private Vector3 anchorPosition;
    private Vector3 endPosition;
    
    public override void Activate(Attack attack, Vector2 direction)
    {
        base.Activate(attack, direction);

        state = AttackState.Ready;
        attack.attackData = selfAttackData;
        
        if (attack.parent is null)
        {
            CreateOtherArrow();
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }
    
    protected override void Update()
    {
        base.Update();

        switch (state)
        {
            case AttackState.Ready:
                state = AttackState.Linear;
                targetStartTime = Time.time + linearDuration;
                return;
            
            case AttackState.Linear:
                attack.transform.position += attack.transform.right * (speed * Time.deltaTime);
                
                if (Time.time > targetStartTime)
                {
                    state = AttackState.Target;
                    var targets = BattleStage.now.GetEnemiesInCircleRangeOrderByDistance(attack.transform.position, 100f, 1);
                    if (targets.Count == 0)
                    {
                        AttackFactory.Instance.Deactivate(attack);
                        return;
                    }
                    
                    target = targets[0];
                    startPosition = attack.transform.position;
                    endPosition = target.transform.position;
                    anchorPosition = Vector3.Lerp(startPosition, endPosition, 0.5f) + attack.transform.right * anchorDelta;
                }
                return;
            
            case AttackState.Target:
                if (target != null)
                    endPosition = target.transform.position;
                
                if (Time.time - targetStartTime > targetDuration)
                {
                    if (target != null)
                        DamageProcessor.ProcessHit(attack, target);
                    AttackFactory.Instance.Deactivate(attack);
                    return;
                }
                
                SetPositionByTarget((Time.time - targetStartTime) / targetDuration);
                break;
        }
    }

    public override void ProcessComponentCollision(Pawn targetPawn)
    {
        base.ProcessComponentCollision(targetPawn);
    }

    private void CreateOtherArrow()
    {
        const int arrowCount = 12;

        attack.transform.rotation = Quaternion.Euler(0, 0, 0);
        for (int i = 1; i < arrowCount; i++)
        {
            var childAttack = AttackFactory.Instance.Create(selfAttackData, attack.attacker, attack, Vector2.zero);
            childAttack.transform.rotation = Quaternion.Euler(0, 0, 360f * i / arrowCount);
        }
    }

    private void SetPositionByTarget(float t)
    {
        var a = Vector3.Lerp(startPosition, anchorPosition, t);
        var b = Vector3.Lerp(anchorPosition, endPosition, t);
        var nextPosition = Vector3.Lerp(a, b, t);

        var direction = nextPosition - attack.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        attack.transform.position = nextPosition;
        attack.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
