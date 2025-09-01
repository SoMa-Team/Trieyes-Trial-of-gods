using System;
using AttackComponents;
using AttackSystem;
using CharacterSystem;
using GamePlayer;
using PrimeTween;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BAC00202_StoneSummon : AttackComponent
{
    [SerializeField] private Collider2D stoneCollider;
    [SerializeField] private Transform stoneSpriteTransform;
    [SerializeField] private SpriteRenderer stoneSprite;
    [SerializeField] private ParticleSystem stoneParticle;
    
    private int stoneCount = 18;
    private float stoneDropBaseRadius = 5f;
    private float stoneDropNoiseRadius = 3f;
    private float stoneDropNoiseAngle = 20f;
    private float stoneDropHeight = 1f;
    private float stoneDropDuration = 0.5f;
    
    public override void Activate(Attack attack, Vector2 direction)
    {
        base.Activate(attack, direction);

        if (attack.parent is null)
        {
            stoneCollider.enabled = false;
            stoneSprite.enabled = false;
            SummonStone();
            Destroy(attack.gameObject);
            return;
        }

        stoneCollider.enabled = true;
        stoneSprite.enabled = true;
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            var pawn = other.GetComponent<Pawn>();
            attack.ProcessAttackCollision(pawn);
            
            Destroy(attack); // TODO: 폭발
        }
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
            
            (childAttack.components[0] as BAC00202_StoneSummon)?.DoSummonAnimation();
        }
    }

    private void DoSummonAnimation()
    {
        var targetY = stoneSpriteTransform.position.y;
        
        stoneSpriteTransform.position += new Vector3(0, stoneDropHeight, 0);
        var color = stoneSprite.color;
        color.a = 0;
        stoneSprite.color = color;
        
        var sequence = Sequence.Create();
        sequence.Chain(Tween.PositionY(stoneSpriteTransform.transform, targetY, stoneDropDuration, Ease.Linear));
        sequence.Group(Tween.Alpha(stoneSprite, 0.5f, stoneDropDuration, Ease.Linear));
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
