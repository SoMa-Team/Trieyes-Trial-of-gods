using UnityEngine;
using System.Linq;
using AttackSystem;
using Stats;

namespace CharacterSystem
{
    public class Character001 : Pawn
    {
        // ===== [기능 1] 고유 필드 =====
        public int experience = 0;
        public new int gold = 0;

        // ===== [기능 2] 초기화 및 스탯 =====
        protected override void Awake()
        {
            base.Awake();
            initBaseStat();
            Debug.Log("Character001 Awake.");
        }

        protected override void initBaseStat()
        {
            SetStatValue(StatType.AttackPower, 100);
            SetStatValue(StatType.Defense, 100);
            SetStatValue(StatType.Health, 100);
            SetStatValue(StatType.CriticalRate, 5);
            SetStatValue(StatType.CriticalDamage, 150);
        }

        // ===== [기능 3] 이벤트 처리 =====
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param); // 부모의 이벤트 전파 로직 먼저 호출

            // Character001 고유의 이벤트 처리
            switch (eventType)
            {
                case Utils.EventType.OnLevelUp:
                    break;
                case Utils.EventType.OnDeath:
                    // Character001의 사망 이벤트는 base.OnEvent에서 이미 처리됨
                    // 여기서는 고유한 추가 로직만 수행
                    if (param as Pawn == this)
                    {
                        Debug.Log($"<color=yellow>{gameObject.name} (Character001) is performing its unique death action: Game Over!</color>");
                    }
                    break;
            }
        }                                                               
    }
} 