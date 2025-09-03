using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleSystem;
using CharacterSystem.Enemies;
using UnityEngine;
using Utils;

namespace CharacterSystem
{
    public class B002_Controller : Controller
    {
        private B002_Water boss;
        private Character target;
        
        public override void Activate(Pawn pawn)
        {
            base.Activate(pawn);
            boss = pawn as B002_Water;
            target = BattleStage.now.mainCharacter;
            attackQueue = new Queue<(float, B002AttackType)>();
            targetTime = Time.time;
            stoneSummonAvailableTime = Time.time + stoneSummonDuration;
            state = B002BehaviorState.Move;
        }
        
        private void Update()
        {
            if (lockMovement)
                return;
            Behavior();
        }

        private enum B002BehaviorState
        {
            Ready,
            Attack,
            Move,
        }
        
        private B002BehaviorState state;
        private Queue<(float, B002AttackType)> attackQueue;
        private readonly List<(float, B002AttackType)> pool = new()
        {
            (1f, B002AttackType.Default),
            (0f, B002AttackType.CircularSector),
        };

        float targetTime;
        private float stoneSummonAvailableTime;
        
        [SerializeField] float moveTime = 1f;
        [SerializeField] float minimumDistanceWithTarget = 1f;
        [SerializeField] float stoneSummonDuration = 20f;
        
        private void Behavior()
        {
            // Queue Add
            if (attackQueue.Count == 0)
            {
                var poolCopy = pool.ToList();
                poolCopy.Shuffle();
                foreach (var attackInfo in poolCopy)
                {
                    attackQueue.Enqueue(attackInfo);   
                }
            }

            if (Time.time >= stoneSummonAvailableTime)
            {
                attackQueue.Enqueue((0f, B002AttackType.StoneSummon));
                stoneSummonAvailableTime = Time.time + stoneSummonDuration;
            }
            
            // Queue Resolve
            switch (state)
            {
                case B002BehaviorState.Ready:
                    boss.Move(Vector2.zero);
                    if (Time.time < targetTime)
                        break;
                    
                    var attackInfo = attackQueue.Dequeue();
                    boss.ExecuteBossAttack(attackInfo.Item2);
                    state = B002BehaviorState.Attack;
                    break;
                
                case B002BehaviorState.Attack:
                    if (boss.isDoAttack)
                        break;
                    state = B002BehaviorState.Move;
                    targetTime = Time.time + moveTime;
                    break;
                
                case B002BehaviorState.Move:
                    var distance = Vector2.Distance(boss.transform.position, target.transform.position);
                    
                    if (Time.time >= targetTime)
                    {
                        state = B002BehaviorState.Ready;
                        targetTime = Time.time + attackQueue.First().Item1;
                    }

                    if (distance < minimumDistanceWithTarget)
                    {
                        targetTime -= Time.deltaTime;
                    }
                    
                    var direction = target.transform.position - boss.transform.position;
                    boss.Move(direction.normalized);
                    moveDir = direction.normalized;
                    break;
            }
        }
    }
}