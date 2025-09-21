using AttackComponents;
using AttackSystem;
using CharacterSystem;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class RAC3001_AttackPet : AttackComponent
{
    private static readonly int AnimationKeyAttack = Animator.StringToHash("Attack");
    private static readonly int AnimationKeyMove = Animator.StringToHash("Move");
    private static readonly int AnimationKeyHorizontal = Animator.StringToHash("Horizontal");

    private enum State
    {
        Casting,
        Attack,
        Move,
    }
    
    [SerializeField] private AttackData subAttack;
    [SerializeField] private Animator animator;
    
    private Pawn target;

    private const float castingDuration = 0.2f;
    private const float moveDuration = 0.8f;
    private const float dontMoveDistance = 1f;
    
    private State state;
    private float nextTriggerTime;
    
    public override void Activate(Attack attack, Vector2 direction)
    {
        base.Activate(attack, direction);
        target = attack.attacker;
        
        state = State.Casting;
        nextTriggerTime = Time.time + moveDuration;
        
        animator.SetBool(AnimationKeyAttack, true);
        animator.SetBool(AnimationKeyMove, false);
        animator.SetFloat(AnimationKeyHorizontal, 1);
    }

    public override void Deactivate()
    {
        target = null;
        base.Deactivate();
    }

    protected override void Update()
    {
        base.Update();
        
        if ((target.transform.position - attack.transform.position).x >= 0)
            animator.SetFloat(AnimationKeyHorizontal, 1);
        else
            animator.SetFloat(AnimationKeyHorizontal, -1);

        switch (state)
        {
            case State.Casting:
                Move();
                
                if (Time.time > nextTriggerTime)
                {
                    state = State.Attack;
                }
                break;
            
            case State.Attack:
                AttackFactory.Instance.Create(subAttack, attack.attacker, attack, Vector2.zero);
                state = State.Move;
                nextTriggerTime += moveDuration;
                
                animator.SetBool(AnimationKeyAttack, false);
                animator.SetBool(AnimationKeyMove, true);
                break;
            
            case State.Move:
                Move();
                
                if (Time.time > nextTriggerTime)
                {
                    state = State.Casting;
                    nextTriggerTime += castingDuration;
                    
                    animator.SetBool(AnimationKeyMove, false);
                    animator.SetBool(AnimationKeyAttack, true);
                }
                break;
        }
    }

    private void Move()
    {
        var distance = Vector2.Distance(target.transform.position, attack.transform.position);
        if (distance > dontMoveDistance)
        {
            attack.transform.position = Vector2.Lerp(target.transform.position, attack.transform.position, Mathf.Pow(0.2f, Time.deltaTime));
        }
    }
}
