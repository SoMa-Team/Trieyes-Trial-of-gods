using System.Collections.Generic;
using Utils;

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
            base.OnEvent(eventType, param); // 부모 클래스의 OnEvent 호출

            if (eventType == Utils.EventType.OnLevelUp)
            {
                // 레벨업 시 공격력 증가
                if (param is int level)
                {
                    // AttackData의 스탯 수정 로직
                }
            }
            
            if (eventType == Utils.EventType.OnStatChange)
            {
                // 스탯 변경 시 추가 효과
            }
        }
    }
} 