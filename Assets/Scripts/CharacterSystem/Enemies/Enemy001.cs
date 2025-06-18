using UnityEngine;
using Utils;

namespace CharacterSystem
{
    public class Enemy001 : Pawn
    {
        // ... existing code ...

        // ===== [기능 1] 초기화 =====
        protected override void Awake()
        {
            base.Awake();
            // TO-DO: AttackComponent 할당
            Debug.Log("Enemy001 Awake.");
        }

        // ===== [기능 2] 사망 이벤트 처리 =====
        protected void OnSelfDeath(object param)
        {
            Debug.Log($"<color=green>{gameObject.name} (Enemy001) is performing its unique death action: Exploding!</color>");
        }

        // ===== [기능 3] 이벤트 처리 =====
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

        // ... existing code ...
    }
} 