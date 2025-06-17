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

        // 자동완성 및 IDE 친화를 위해 주요 스탯은 별도 프로퍼티를 제공합니다.
        public IntegerStatValue AttackPower => stats[StatType.AttackPower];
        public IntegerStatValue AttackSpeed => stats[StatType.AttackSpeed];
        public IntegerStatValue Health => stats[StatType.Health];
        public IntegerStatValue Defense => stats[StatType.Defense];
        public IntegerStatValue MoveSpeed => stats[StatType.MoveSpeed];
        // 자주 사용하는 것은 추가하면 간편하게 사용할 수 있습니다.
    }
} 