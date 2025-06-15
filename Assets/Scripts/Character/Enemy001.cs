using CharacterSystem;
using Core;
using AttackSystem;
using UnityEngine;

namespace CharacterSystem
{
    public class Enemy001 : Pawn
    {
        // ... existing code ...

        protected override void Awake()
        {
            base.Awake();
            // TO-DO: AttackComponent 할당

            // 적 자신의 사망 이벤트를 구독합니다.
            // Pawn의 TriggerEvent(Core.EventType.OnDeath)가 호출될 때 OnSelfDeath 메서드가 실행됩니다.
            RegisterEvent(Core.EventType.OnDeath, OnSelfDeath);

            Debug.Log("Enemy001 Awake. OnDeath event registered.");
        }

        // ... existing code ...

        /// <summary>
        /// 이 적(Pawn)이 사망했을 때 호출되는 이벤트 핸들러입니다.
        /// Pawn의 TriggerEvent(Core.EventType.OnDeath)에 의해 실행됩니다.
        /// </summary>
        /// <param name="param">사망한 Pawn 인스턴스 (this)</param>
        protected void OnSelfDeath(object param)
        {
            // 이 적의 고유한 사망 로직을 여기에 구현합니다.
            // 예: 폭발 애니메이션 재생, 아이템 드롭, 점수 추가 등
            Debug.Log($"<color=green>{gameObject.name} (Enemy001) is performing its unique death action: Exploding!</color>");
            // 시체 폭발 효과 (예시)
        }

        // ... existing code ...
    }
} 