using System;
using System.Collections.Generic;
using AttackComponents;
using AttackSystem;
using BattleSystem;
using CharacterSystem;
using RelicSystem;
using UnityEngine;

public class BAC00202_CircularSector : AttackComponent
{
    enum AttackMode
    {
        Telegraph,
        Attack
    };

    [SerializeField] private AttackData childAttackData;
    [SerializeField] private LineRenderer lineRenderer;
    
    // Constants
    private float centralAngle = 40f;
    private float TelegraphSize = 10f;
    private float attackDelay = 1f;
    private float minimumAngle = 5f;
    private int childScale = 200;
    
    private AttackMode mode;
    private float startTime;
    
    private Vector2 startDirection;

    public override void Activate(Attack attack, Vector2 direction)
    {
        mode = AttackMode.Telegraph;
        startTime = Time.time;
        this.startDirection = direction;
        
        base.Activate(attack, direction);
        DrawLine();
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    public override void ProcessComponentCollision(Pawn targetPawn)
    {
        base.ProcessComponentCollision(targetPawn);
    }

    protected override void Update()
    {
        base.Update();
        
        if (lineRenderer is null)
            return;

        switch (mode)
        {
            case AttackMode.Telegraph:
                if (Time.time - startTime < attackDelay)
                    break;
                mode = AttackMode.Attack;
                break;
            
            case AttackMode.Attack:
                makeChildAttacks();
                AttackFactory.Instance.Deactivate(attack);
                break;
        }
    }

    private void DrawLine()
    {
        var cx = Mathf.Cos(centralAngle / 2 * Mathf.Deg2Rad);
        var cy = Mathf.Sin(centralAngle / 2 * Mathf.Deg2Rad);
        
        lineRenderer.positionCount = 3;
        lineRenderer.SetPosition(0, transform.position + transform.TransformVector(TelegraphSize * new Vector3(cx, cy, 0)));
        lineRenderer.SetPosition(1, transform.position);
        lineRenderer.SetPosition(2, transform.position + transform.TransformVector(TelegraphSize * new Vector3(cx, -cy, 0)));
    }

    private void makeChildAttacks()
    {
        var count = Mathf.Ceil(centralAngle / minimumAngle) + 1;

        var baseAngle = Mathf.Atan2(startDirection.y, startDirection.x) * Mathf.Rad2Deg;
        for (int i = 0; i < count; i++)
        {
            var angle = (centralAngle * i / (count - 1)) + baseAngle - centralAngle / 2;
            var direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)); 
            var child = AttackFactory.Instance.Create(childAttackData, attack.attacker, attack, direction, new Dictionary<RelicStatType, int>
            {
                {RelicStatType.AOE, 100},
                {RelicStatType.Range, 25},
            }, true);
        }
    }
}
