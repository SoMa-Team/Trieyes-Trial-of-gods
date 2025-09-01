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

        public override float vfxYOffset { get { return 0f; } }
   
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
        
        // TODO
        public override bool ExecuteAttack(PawnAttackType attackType = PawnAttackType.BasicAttack)
        {
            var direction = (playerTarget.transform.position - transform.position).normalized;
            switch (attackType)
            {
                case PawnAttackType.BasicAttack:
                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        CalculateAttackCooldown();
                        lastAttackTime = Time.time;
                        AttackFactory.Instance.Create(basicAttack, this, null, direction); 
                        return true;
                    }
                    return false;
                case PawnAttackType.Skill1:
                    if (CheckSkillCooldown(PawnAttackType.Skill1))
                    {
                        lastSkillAttack1Time = Time.time;
                        Attack temp = AttackFactory.Instance.Create(skill1Attack, this, null, direction);
                        Debug.Log($"<color=yellow>[SKILL1] {temp.gameObject.name} skill1Attack: {temp.attackData.attackId}, attacker: {temp.attacker.gameObject.name}</color>");
                        return true;
                    }
                    Debug.Log($"<color=yellow>[SKILL1] {gameObject.name} skillAttack1Cooldown: {skillAttack1Cooldown}, lastSkillAttack1Time: {lastSkillAttack1Time}</color>");
                    return false;

                case PawnAttackType.Skill2:
                    if (CheckSkillCooldown(PawnAttackType.Skill2))
                    {
                        lastSkillAttack2Time = Time.time;
                        Attack temp = AttackFactory.Instance.Create(skill2Attack, this, null, LastMoveDirection);
                        Debug.Log($"<color=yellow>[SKILL2] {temp.gameObject.name} skill2Attack: {temp.attackData.attackId}, attacker: {temp.attacker.gameObject.name}</color>");
                        return true;
                    }
                    Debug.Log($"<color=yellow>[SKILL2] {gameObject.name} skillAttack2Cooldown: {skillAttack2Cooldown}, lastSkillAttack2Time: {lastSkillAttack2Time}</color>");
                    return false;
                    
                default:
                    return false;
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
            
            // 골드 드랍 로직 (임시로 플레이어에게 직접 전달)
            // TODO: 실제로는 드롭 아이템 시스템을 통해 구현해야 함
            Debug.Log("OnSelfDeath Called");
            Debug.Log($"{result.attacker}");
            if (result.attacker != null)
            {
                Gold gold = DropFactory.Instance.CreateGold(transform.position, dropGold);
                BattleStage.now.AttachGold(gold);
                // result.attacker.ChangeGold(dropGold);
                Debug.Log($"<color=yellow>{gameObject.name} dropped {dropGold} gold to {result.attacker.gameObject.name}</color>");
                Debug.Log($"Player Gold: {result.attacker.gold}");
            }
        }
    }
} 
