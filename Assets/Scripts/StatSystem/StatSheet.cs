using System;
using System.Collections.Generic;

namespace Stats
{
    /// <summary>
    /// 스탯 정보를 담는 클래스입니다.
    /// </summary>
    public class StatSheet
    {
        // --- 필드 ---

        private readonly Dictionary<StatType, IntegerStatValue> stats
            = new Dictionary<StatType, IntegerStatValue>();

        const int DEFAULT_VALUE = 0;

        // --- 생성자 ---

        /// StatSheet의 새 인스턴스를 초기화합니다.
        /// 모든 StatType에 대해 기본값(0)으로 초기화됩니다.
        public StatSheet()
        {
            // Enum 전체를 순회하며 초기화 (기본값 필요 시 변경)
            foreach (StatType type in Enum.GetValues(typeof(StatType)))
            {
                stats[type] = new IntegerStatValue(DEFAULT_VALUE);
            }
        }

        // --- 인덱서 ---

        /// StatType으로 해당 스탯 값을 조회합니다.
        public IntegerStatValue this[StatType type] => stats[type];
    }
}
