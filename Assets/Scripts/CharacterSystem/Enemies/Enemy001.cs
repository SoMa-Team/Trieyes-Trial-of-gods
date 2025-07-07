using AttackSystem;
using UnityEngine;
using Utils;
using System.Linq;
using BattleSystem;

namespace CharacterSystem
{
    /// <summary>
    /// 사망 시 골드를 드랍하는 기본 적 캐릭터
    /// </summary>
    public class Enemy001 : Pawn
    {
        // ===== [기능 1] 적 기본 정보 =====
        [SerializeField] 
        private int dropGold = 10; // 드랍할 골드 양
        private BoxCollider2D boxCollider;
        
        // ===== [기능 2] 초기화 =====
        protected override void Awake()
        {
            base.Awake();

            // Collision Layer를 Enemy로 설정
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }

        protected override void Start()
        {
            base.Start();
        }

        // ===== [커스텀 메서드] =====
        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public override void Activate()
        {
            base.Activate();
            // TODO: AttackComponent 할당
            //Debug.Log("Enemy001 Activated.");

            // 이런 느낌으로 각 적마다 커스터마이징 
            // boxCollider = Collider as BoxCollider2D;
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public override void Deactivate()
        {
            // Enemy001 고유 정리 로직
            dropGold = 10; // 기본값으로 초기화
            
            base.Deactivate();
            //Debug.Log("Enemy001 Deactivated.");
        }

        /// <summary>
        /// 이벤트를 처리합니다.
        /// </summary>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="param">이벤트 파라미터</param>
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param);
            switch (eventType)
            {
                case Utils.EventType.OnDeath:
                    OnSelfDeath(param);
                    break;
                // 기타 이벤트별 동작 추가
            }
        }

        // ===== [이벤트 처리 메서드] =====
        /// <summary>
        /// 사망 시 골드 드랍 처리
        /// </summary>
        /// <param name="param">이벤트 파라미터</param>
        protected void OnSelfDeath(object param)
        {
            //Debug.Log($"<color=green>{gameObject.name} (Enemy001) is performing its unique death action: Exploding!</color>");
            
            // 골드 드랍 로직 (임시로 플레이어에게 직접 전달)
            // TODO: 실제로는 드롭 아이템 시스템을 통해 구현해야 함
            if (param is Pawn.AttackEventData attackData && attackData.attacker != null)
            {
                attackData.attacker.ChangeGold(dropGold);
                //Debug.Log($"<color=yellow>{gameObject.name} dropped {dropGold} gold to {attackData.attacker.gameObject.name}</color>");
            }
        }
    }
} 
