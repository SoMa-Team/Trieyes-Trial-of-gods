using System.Collections.Generic;
using Stats;

namespace CardSystem
{
    /// <summary>
    /// 카드의 속성별 스탯 정보를 관리하는 클래스
    /// </summary>
    public class CardStat
    {
        // ===== [기능 1] 카드 스탯 정보 및 생성 =====
        public Property PropertyType { get; private set; }
        public List<StatInfo> StatInfos { get; private set; }

        /// <summary>
        /// CardStat의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="property">이 CardStat이 대표하는 속성</param>
        /// <param name="level">이 CardStat의 레벨. IntegerStatValue 타입.</param>
        public CardStat(Property property, IntegerStatValue level)
        {
            PropertyType = property;
            StatInfos = new List<StatInfo>();
            SetStatInfo(property, level);
        }

        // ===== [기능 2] 스탯 정보 설정 =====
        /// <summary>
        /// 주어진 속성과 레벨에 따라 StatInfo 목록을 설정합니다.
        /// 실제 스탯 계산 로직은 여기에 구현됩니다.
        /// </summary>
        /// <param name="property">속성</param>
        /// <param name="level">레벨</param>
        private void SetStatInfo(Property property, IntegerStatValue level)
        {
            // 기존 StatInfos를 초기화합니다.
            StatInfos.Clear();

            // 예시: 레벨에 따라 스탯 값을 계산합니다.
            // 실제 게임 디자인에 따라 복잡한 공식이 들어갈 수 있습니다.
            switch (property)
            {
                default:
                    // 처리되지 않은 속성에 대한 기본값 또는 오류 처리
                    break;
            }
        }
    }
} 