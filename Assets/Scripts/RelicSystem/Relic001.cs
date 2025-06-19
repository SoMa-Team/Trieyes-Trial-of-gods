using System.Collections.Generic;
using Utils;
using Stats;
using CharacterSystem;
using UnityEngine;

namespace RelicSystem
{
    public class Relic001 : Relic
    {
        // ===== [기능 1] 생성자 및 이벤트 등록 =====
        public Relic001() : base(new RelicInfo 
        { 
            name = "신비한 검",
            relicId = 1,
            compIds = new List<int> { 1, 2 } // 기본 공격과 스킬1에 영향을 줌
        })
        {
            // 이벤트 핸들러 등록 코드 삭제됨
        }

        // ===== [기능 2] 이벤트 처리 =====
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            switch (eventType)
            {
                case Utils.EventType.OnLevelUp:
                    OnLevelUpHandler(param);
                    break;
                case Utils.EventType.OnStatChange:
                    OnStatChangeHandler(param);
                    break;
                case Utils.EventType.OnAttack:
                    OnAttackHandler(param);
                    break;
                // 기타 이벤트별 동작 추가
            }
        }

        private void OnLevelUpHandler(object param)
        {
            // 레벨업 시 공격력 증가
            if (param is int level && owner != null)
            {
                owner.IncreaseStatValue(StatType.AttackPower, 5);
                Debug.Log($"<color=purple>[RELIC001] {owner.gameObject.name} gained 5 AttackPower from level up</color>");
            }
        }

        private void OnStatChangeHandler(object param)
        {
            // 스탯 변경 시 추가 효과
            if (param is StatInfo statInfo && owner != null)
            {
                Debug.Log($"<color=purple>[RELIC001] {owner.gameObject.name} stat changed: {statInfo.Type}</color>");
            }
        }

        private void OnAttackHandler(object param)
        {
            // 공격 시 추가 효과
            if (param is Pawn.AttackEventData attackData && owner != null)
            {
                Debug.Log($"<color=purple>[RELIC001] {owner.gameObject.name} attacked {attackData.target.gameObject.name}</color>");
            }
        }
    }
} 