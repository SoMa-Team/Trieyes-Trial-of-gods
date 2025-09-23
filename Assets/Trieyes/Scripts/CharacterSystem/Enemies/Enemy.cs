using AttackSystem;
using UnityEngine;
using Utils;
using System.Linq;
using BattleSystem;
using Stats;
using System.Collections.Generic;

namespace CharacterSystem
{
    /// <summary>
    /// 사망 시 골드를 드랍하는 기본 적 캐릭터
    /// </summary>
    public class Enemy : Pawn
    {
        // ===== [기능 1] 적 기본 정보 =====
        [SerializeField] 
        protected int dropGold; // 드랍할 골드 양
        public Character playerTarget;

        public override Vector2 CenterOffset { get; set; } = Vector2.zero;
   
        // ===== [기능 2] 초기화 =====
        protected override void Start()
        {
            base.Start();

            // Collision Layer를 Enemy로 설정
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }

        // ===== [커스텀 메서드] =====
        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public override void Activate()
        {
            base.Activate();
            playerTarget = BattleStage.now.mainCharacter;
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public override void Deactivate()
        {        
            base.Deactivate();
            ////Debug.Log("Enemy001 Deactivated.");
        }

        /// <summary>
        /// 이벤트를 처리합니다.
        /// </summary>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="param">이벤트 파라미터</param>
        public override bool OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param);
            switch (eventType)
            {
                case Utils.EventType.OnDeath:
                    Debug.Log("OnDeath Event Activated");
                    OnSelfDeath(param as AttackResult);
                    return true;

                // 기타 이벤트별 동작 추가
                default:
                    return false;
            }
        }
        
        public virtual bool ExecuteAttack(PawnAttackType attackType = PawnAttackType.BasicAttack)
        {
            if(CheckCooldown(attackType))
            {
                switch (attackType)
                {
                    case PawnAttackType.BasicAttack:
                        // t2 = t1;
                        // t1 = Time.time;
                        // Debug.LogError($"delta time : {t1 - t2}");
                        lastAttackTime = Time.time;
                        ChangeAnimationState("ATTACK");
                        CreateAttack(PawnAttackType.BasicAttack);
                        return true;
                    case PawnAttackType.Skill1:
                        if (skill1Attack is null)
                        {
                            return false;
                        }
                        lastSkillAttack1Time = Time.time;
                        ChangeAnimationState("SKILL001");
                        CreateAttack(PawnAttackType.Skill1);
                        return true;

                    case PawnAttackType.Skill2:
                        if (skill2Attack is null)
                        {
                            return false;
                        }
                        lastSkillAttack2Time = Time.time;
                        ChangeAnimationState("SKILL002");
                        CreateAttack(PawnAttackType.Skill2);
                        return true;
                        
                    default:
                        return false;
                }
            }
            return false;
        }
        
        // TODO
        public void CreateAttack(PawnAttackType attackType)
        {
            switch (attackType)
            {
                case PawnAttackType.BasicAttack:
                    AttackFactory.Instance.Create(basicAttack, this, null, LastMoveDirection); 
                    break;
                case PawnAttackType.Skill1:
                    AttackFactory.Instance.Create(skill1Attack, this, null, LastMoveDirection);
                    break;
                case PawnAttackType.Skill2:
                    AttackFactory.Instance.Create(skill2Attack, this, null, LastMoveDirection);
                    break;
                default:
                    break;
            }
        }

        // ===== [이벤트 처리 메서드] =====
        /// <summary>
        /// 사망 시 골드 드랍 처리
        /// </summary>
        /// <param name="param">이벤트 파라미터</param>
        protected virtual void OnSelfDeath(AttackResult result)
        {
            ////Debug.Log($"<color=green>{gameObject.name} (Enemy001) is performing its unique death action: Exploding!</color>");
            
            Debug.Log("OnSelfDeath Called");
            Debug.Log($"{result.attacker}");
            if (result.attacker != null)
            {
                var realDropGold = dropGold;
                if (Random.Range(0, 100f) < result.attacker.statSheet.Get(StatType.GoldDropRate))
                    realDropGold += dropGold;
                Gold gold = DropFactory.Instance.CreateGold(transform.position, realDropGold);
                BattleStage.now.AttachGold(gold);
                
                Debug.Log($"<color=yellow>{gameObject.name} dropped {dropGold} gold to {result.attacker.gameObject.name}</color>");
                Debug.Log($"Player Gold: {result.attacker.gold}");

                // BattleStage가 BreakThrough 모드인 경우
                if (BattleStage.now is BattleBreakThroughStage)
                {
                    (BattleStage.now as BattleBreakThroughStage).BreakThroughCountDown();
                }
            }
        }
    }
} 
