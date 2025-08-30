using AttackComponents;
using AttackSystem;
using CharacterSystem;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;

public class BAC00202_StoneSummon : AttackComponent
{
    [SerializeField] private Collider2D stoneCollider;
    [SerializeField] private Transform stoneSpriteTransform;
    [SerializeField] private SpriteRenderer stoneSprite;
    [SerializeField] private ParticleSystem stoneParticle;
    
    [SerializeField] private int stoneCount = 10;
    [SerializeField] private float stoneDropBaseRadius = 3f;
    [SerializeField] private float stoneDropNoiseRadius = 0.5f;
    [SerializeField] private float stoneDropNoiseAngle = 5f;
    [SerializeField] private float stoneDropHeight = 1f;
    [SerializeField] private float stoneDropDuration = 0.2f;
    
    public override void Activate(Attack attack, Vector2 direction)
    {
        base.Activate(attack, direction);

        if (attack.parent is null)
        {
            stoneCollider.enabled = false;
            stoneSprite.enabled = false;
            SummonStone();
            return;
        }

        stoneCollider.enabled = false;
        stoneSprite.enabled = true;
        DoSummonAnimation();
    }

    public override void Deactivate()
    {
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

    public void SummonStone()
    {
        for (int i = 0; i < stoneCount; i++)
        {
            var baseAngle = 360 * i / stoneCount;
            var angle = baseAngle + Random.Range(-stoneDropNoiseAngle, stoneDropNoiseAngle);
            var radius = stoneDropBaseRadius + Random.Range(-stoneDropNoiseRadius, stoneDropNoiseRadius);
            
            var targetPosition = transform.position + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, Mathf.Sin(angle * Mathf.Deg2Rad) * radius, 0);
            var childAttack = AttackFactory.Instance.Create(attack.attackData, attack.attacker, attack, Vector2.zero);
            childAttack.transform.position = targetPosition;
        }
    }

    private void DoSummonAnimation()
    {
        var targetY = stoneSpriteTransform.position.y;
        stoneSpriteTransform.position -= new Vector3(0, stoneDropHeight, 0);

        var sequence = Sequence.Create();
        sequence.Chain(Tween.Alpha(stoneSprite, 0f, 0));
        sequence.Chain(Tween.PositionY(stoneSpriteTransform.transform, targetY, stoneDropDuration, Ease.Linear));
        sequence.Group(Tween.Alpha(stoneSprite, 1f, stoneDropDuration));
        sequence.Group(Tween.Delay(stoneParticle, 0, particle =>
        {
            particle.Emit(1);
        }));
        sequence.Chain(Tween.Delay(stoneCollider, 0, stoneCollider =>
        {
            stoneCollider.enabled = true;
        }));
    }
}
