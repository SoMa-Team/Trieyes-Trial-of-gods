using System.Collections.Generic;
using Utils;

namespace RelicSystem
{
    /// <summary>
    /// 게임 내 유물을 나타내는 클래스입니다.
    /// 유물은 자체적으로 이벤트를 등록하고 처리할 수 있는 IEventHandler를 구현합니다.
    /// </summary>
    public class Relic : IEventHandler
    {
        // ===== [기능 1] 유물 정보 및 생성 =====
        public RelicInfo info;
        public Relic(RelicInfo info)
        {
            this.info = info;
        }

        // ===== [기능 2] 이벤트 처리 =====
        public virtual void OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                // 예시: 유물 관련 이벤트별 동작 하드코딩
                case EventType.OnLevelUp:
                    // 레벨업 시 효과
                    break;
                case EventType.OnStatChange:
                    // 스탯 변경 시 효과
                    break;
                // 기타 이벤트별 동작 추가
            }
        }
    }
} 