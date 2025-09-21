using System;
using System.Collections.Generic;
using AttackComponents;
using AttackSystem;
using BattleSystem;
using CharacterSystem;
using UnityEngine;

public class RAC3002_GuidedMissile : AttackComponent
{
    private Pawn target;

    [SerializeField] private Collider2D collider;
    [SerializeField] private TrailRenderer trailRenderer;

    private readonly float attackRadius = 50f;
    private readonly float attackSpeed = 10f;
    private readonly float explodeRadius = 0.1f;

    public override void Activate(Attack attack, Vector2 direction)
    {
        base.Activate(attack, direction);

        var enemies =
            BattleStage.now.GetEnemiesInCircleRangeOrderByDistance(attack.transform.position, attackRadius, 1);
        if (enemies.Count == 0)
        {
            AttackFactory.Instance.Deactivate(attack);
            return;
        }

        target = enemies[0];
        
        trailRenderer.Clear();
    }

    public override void Deactivate()
    {
        base.Deactivate();

        target = null;
    }

    protected override void Update()
    {
        base.Update();

        var distance = Vector2.Distance(target.transform.position, attack.transform.position);
        if (distance < explodeRadius)
        {
            List<Collider2D> colliders = new List<Collider2D>();
            collider.Overlap(colliders);

            foreach (var targetCollider in colliders)
            {
                if (!targetCollider.CompareTag("Enemy"))
                    continue;
                
                var enemy = targetCollider.gameObject.GetComponent<Enemy>();
                DamageProcessor.ProcessHit(attack, enemy);
            }

            AttackFactory.Instance.Deactivate(attack);
            return;
        }

        var direction = (target.transform.position - attack.transform.position).normalized;
        attack.transform.position += direction * (attackSpeed * Time.deltaTime);
    }

    public override void ProcessComponentCollision(Pawn targetPawn)
    {
        base.ProcessComponentCollision(targetPawn);
    }
}