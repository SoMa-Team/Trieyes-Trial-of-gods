using System.Collections.Generic;
using Stats;
using UnityEngine;

namespace Utils
{
    public class KoreanToStatType
    {
        private static readonly Dictionary<string, StatType> _korToStatType = new()
        {
            {"공격력", StatType.AttackPower},
            {"방어력", StatType.Defense},
            {"사정거리", StatType.AttackRange},
            {"공격속도", StatType.AttackSpeed},
            {"체력",   StatType.Health},
            // 필요시 더 추가
        };
        public static StatType ToStatType(string korean)
        {
            if (_korToStatType.TryGetValue(korean, out var statType))
                return statType;
            Debug.LogWarning($"StatTypeUtils: 알 수 없는 한글 스탯명 '{korean}' (Health로 대체)");
            return StatType.Health; // 디폴트
        }
    }
}