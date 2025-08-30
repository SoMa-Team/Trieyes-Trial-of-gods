using System;
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
        private bool autoMode = false;
        
        public override void Activate(Pawn pawn)
        {
            base.Activate(pawn);
            boss = pawn as B002_Water;
            target = BattleStage.now.mainCharacter;
            attackQueue = new Queue<B002AttackType>();
            targetMoveTime = Time.time;
            state = B002BehaviorState.Move;
        }

        // TODO : Test용 플레이어블 보스
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                autoMode = !autoMode;
                return;
            }

            if (autoMode)
            {
                Behavior();
                return;
            }
            
            var direction = new Vector2(0, 0);
            
            if (Input.GetKey(KeyCode.W))
                direction.y++;
            if (Input.GetKey(KeyCode.A))
                direction.x--;
            if (Input.GetKey(KeyCode.S))
                direction.y--;
            if (Input.GetKey(KeyCode.D))
                direction.x++;
            
            boss.Move(direction.normalized);
            moveDir = direction;
            
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                boss.ExecuteBossAttack(B002AttackType.Default);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                boss.ExecuteBossAttack(B002AttackType.StoneSummon);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                boss.ExecuteBossAttack(B002AttackType.Rush);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                boss.ExecuteBossAttack(B002AttackType.StoneExplode);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                boss.ExecuteBossAttack(B002AttackType.FireDischarge);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                boss.ExecuteBossAttack(B002AttackType.SpawnSlowField);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                boss.ExecuteBossAttack(B002AttackType.CircularSector);
                return;
            }
        }

        private enum B002BehaviorState
        {
            Ready,
            Attack,
            Move,
        }
        
        private B002BehaviorState state;
        private Queue<B002AttackType> attackQueue;
        private readonly List<(float, B002AttackType)> pool = new()
        {
            (0.5f, B002AttackType.Default),
            (0f, B002AttackType.CircularSector),
        };

        float targetMoveTime;
        
        [SerializeField] float moveTime = 1f;
        
        private void Behavior()
        {
            // Queue Add
            if (attackQueue.Count == 0)
            {
                var poolCopy = pool.ToList();
                poolCopy.Shuffle();
                foreach (var type in poolCopy)
                {
                    attackQueue.Enqueue(type.Item2);   
                }
            }
            
            // Queue Resolve
            switch (state)
            {
                case B002BehaviorState.Ready:
                    var attackType = attackQueue.Dequeue();
                    boss.Move(Vector2.zero);
                    boss.ExecuteBossAttack(attackType);
                    state = B002BehaviorState.Attack;
                    break;
                
                case B002BehaviorState.Attack:
                    if (boss.isDoAttack)
                        break;
                    state = B002BehaviorState.Move;
                    targetMoveTime = Time.time + moveTime;
                    break;
                
                case B002BehaviorState.Move:
                    if (Time.time >= targetMoveTime)
                    {
                        state = B002BehaviorState.Ready;
                    }
                    
                    var direction = target.transform.position - boss.transform.position;
                    boss.Move(direction.normalized);
                    moveDir = direction.normalized;
                    break;
            }
        }
    }
}