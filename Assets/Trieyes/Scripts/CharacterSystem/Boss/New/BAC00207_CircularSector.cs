using System;
using AttackComponents;
using AttackSystem;
using BattleSystem;
using CharacterSystem;
using UnityEngine;

public class BAC00207_CircularSector : AttackComponent
{
    enum AttackMode
    {
        Telegraph,
        Attack
    };
    
    [SerializeField] private LineRenderer lineRenderer;

    [Header("======= @@@@@ Test @@@@@ =======")] // TODO : 삭제 필요
    [SerializeField] private float centralAngle;
    [SerializeField] private float TelegraphSize;
    [SerializeField] private float attackDelay;
    
    private AttackMode mode;
    private float startTime;

    public override void Activate(Attack attack, Vector2 direction)
    {
        mode = AttackMode.Telegraph;
        
        base.Activate(attack, direction);
        DrawLine();
        startTime = Time.time;
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
}
