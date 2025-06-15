using System.Collections.Generic;
using Core;
using Utils;

namespace RelicSystem
{
    public class Relic001 : Relic
    {
        public Relic001() : base(new RelicInfo 
        { 
            name = "신비한 검",
            relicId = 1,
            compIds = new List<int> { 1, 2 } // 기본 공격과 스킬1에 영향을 줌
        })
        {
            // 이벤트 핸들러 등록
            RegisterEvent(EventType.OnLevelUp, OnLevelUpHandler);
            RegisterEvent(EventType.OnStatChange, OnStatChangeHandler);
        }

        private void OnLevelUpHandler(object param)
        {
            // 레벨업 시 공격력 증가
            if (param is int level)
            {
                // AttackData의 스탯 수정 로직
            }
        }

        private void OnStatChangeHandler(object param)
        {
            // 스탯 변경 시 추가 효과
            if (param is StatInfo statInfo)
            {
                // 스탯 수정 로직
            }
        }
    }
} 