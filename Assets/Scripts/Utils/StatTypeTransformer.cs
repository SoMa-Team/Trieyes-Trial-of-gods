using System.Collections.Generic;
using Stats;
using UnityEngine;

namespace Utils
{
    public class StatTypeTransformer
    {
        private static readonly Dictionary<string, StatType> korToStatType = new()
        {
            {"공격력", StatType.AttackPower},
            {"방어력", StatType.Defense},
            {"사정거리", StatType.AttackRange},
            {"공격속도", StatType.AttackSpeed},
            {"체력",   StatType.Health},
            // 필요시 더 추가
        };

        private static readonly Dictionary<StatType, string> StatTypeToKor = new()
        {
            { StatType.AttackPower, "공격력" },
            { StatType.Defense, "방어력" },
            { StatType.AttackSpeed, "공격속도" },
            { StatType.AttackRange, "사정거리" },
            { StatType.Health, "체력" }
        };
        public static StatType KoreanToStatType(string korean)
        {
            if (korToStatType.TryGetValue(korean, out var statType))
                return statType;
            Debug.LogWarning($"알 수 없는 한글 스탯명 '{korean}' (Health로 대체)");
            return StatType.Health; // 디폴트
        }

        public static string StatTypeToKorean(StatType statType)
        {
            if(StatTypeToKor.TryGetValue(statType, out var korean))
                return korean;
            Debug.LogWarning($"S한글 명칭이 지정되지 않은 스탯입니다. 체력으로 대체하였습니다.");
            return "체력";
        }
    }
}