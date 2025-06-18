using System;
using System.Collections.Generic;

namespace Stats
{
    // 스탯 정보를 담는 클래스
    public class StatSheet
    {
        private readonly Dictionary<StatType, IntegerStatValue> stats
        = new Dictionary<StatType, IntegerStatValue>();

        public StatSheet(){
            // Enum 전체 루프 돌면서 초기화 (기본값 필요시 지정)
            foreach (StatType type in Enum.GetValues(typeof(StatType)))
            {
                stats[type] = new IntegerStatValue(0); // 기본값 0, 필요시 변경
            }
        }

        // 접근용 인덱서
        public IntegerStatValue this[StatType type] => stats[type];
    }
} 