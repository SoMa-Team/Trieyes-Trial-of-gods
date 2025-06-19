using AttackSystem;
using UnityEngine;
using Utils;

namespace CharacterSystem
{
    public class Enemy001 : Pawn
    {
        // ===== [기능 1] 적 기본 정보 =====
        [SerializeField] private int dropGold = 10; // 드랍할 골드 양
        
        // ===== [기능 2] 초기화 =====
        protected override void Awake()
        {
            base.Awake();
            // TO-DO: AttackComponent 할당
            Debug.Log("Enemy001 Awake.");
        }

        // ===== [기능 3] 사망 이벤트 처리 =====
        protected void OnSelfDeath(object param)
        {
            Debug.Log($"<color=green>{gameObject.name} (Enemy001) is performing its unique death action: Exploding!</color>");
            
            // 골드 드랍 로직 (임시로 플레이어에게 직접 전달)
            // TODO: 실제로는 드롭 아이템 시스템을 통해 구현해야 함
            if (param is Pawn.AttackEventData attackData && attackData.attacker != null)
            {
                attackData.attacker.EarnGold(dropGold);
                Debug.Log($"<color=yellow>{gameObject.name} dropped {dropGold} gold to {attackData.attacker.gameObject.name}</color>");
            }
        }

        // ===== [기능 4] 이벤트 처리 =====
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            switch (eventType)
            {
                case Utils.EventType.OnDeath:
                    OnSelfDeath(param);
                    break;
                // 기타 이벤트별 동작 추가
            }
        }

        public override void TakeAttack(Attack attack)
        {
            throw new System.NotImplementedException();
        }

        public override void PerformAttack(Pawn target, Attack attack)
        {
            throw new System.NotImplementedException();
        }
    }
} 