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

        // --- 생성자 ---

        /// <summary>
        /// StatSheet의 새 인스턴스를 초기화합니다.
        /// 모든 StatType에 대해 기본값(0)으로 초기화됩니다.
        /// </summary>
        public StatSheet()
        {
            // Enum 전체를 순회하며 초기화 (기본값 필요 시 변경)
            foreach (StatType type in Enum.GetValues(typeof(StatType)))
            {
                stats[type] = new IntegerStatValue(0);
            }
        }

        // --- 인덱서 ---

        /// <summary>
        /// StatType으로 해당 스탯 값을 조회합니다.
        /// </summary>
        public IntegerStatValue this[StatType type] => stats[type];
    }
}
