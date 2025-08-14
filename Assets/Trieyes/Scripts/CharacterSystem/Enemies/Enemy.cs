using AttackSystem;
using UnityEngine;
using Utils;
using System.Linq;
using BattleSystem;
using Stats;

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
            // TODO: AttackComponent 할당
            ////Debug.Log("Enemy001 Activated.");

            // 이런 느낌으로 각 적마다 커스터마이징 
            // boxCollider = Collider as BoxCollider2D;
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public override void Deactivate()
        {        
            base.Deactivate();
            ////Debug.Log("Enemy001 Deactivated.");
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);

            if(other.gameObject.CompareTag("Player"))
            {
                var character = other.gameObject.GetComponent<Character>();
                DamageProcessor.ProcessHit(this, character);
            }
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
                    break;
                // 기타 이벤트별 동작 추가
            }

            return false;
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
