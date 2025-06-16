using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    /// <summary>
    /// 스탯 계산을 담당하는 유틸리티 클래스입니다.
    /// </summary>
    public static class StatCalculator
    {
        /// <summary>
        /// 두 스탯 리스트를 합산합니다.
        /// </summary>
        /// <param name="baseStats">기본 스탯 리스트</param>
        /// <param name="additionalStats">추가할 스탯 리스트</param>
        /// <returns>합산된 스탯 리스트</returns>
        public static List<StatInfo> AddStats(List<StatInfo> baseStats, List<StatInfo> additionalStats)
        {
            Dictionary<StatType, StatInfo> combinedStats = new Dictionary<StatType, StatInfo>();

            // 기본 스탯 추가
            foreach (var stat in baseStats)
            {
                combinedStats[stat.type] = new StatInfo(stat.type, stat.value);
            }

            // 추가 스탯 합산
            foreach (var stat in additionalStats)
            {
                if (combinedStats.ContainsKey(stat.type))
                {
                    combinedStats[stat.type].value += stat.value;
                }
                else
                {
                    combinedStats[stat.type] = new StatInfo(stat.type, stat.value);
                }
            }

            return combinedStats.Values.ToList();
        }

        /// <summary>
        /// 스탯 리스트에서 특정 타입의 스탯 값을 가져옵니다.
        /// </summary>
        /// <param name="stats">스탯 리스트</param>
        /// <param name="statType">찾을 스탯 타입</param>
        /// <returns>스탯 값, 없으면 0</returns>
        public static float GetStatValue(List<StatInfo> stats, StatType statType)
        {
            var stat = stats.FirstOrDefault(s => s.type == statType);
            return stat?.value ?? 0f;
        }

        /// <summary>
        /// 스탯 리스트에서 특정 타입의 스탯을 설정합니다.
        /// </summary>
        /// <param name="stats">스탯 리스트</param>
        /// <param name="statType">설정할 스탯 타입</param>
        /// <param name="value">설정할 값</param>
        public static void SetStatValue(List<StatInfo> stats, StatType statType, float value)
        {
            var stat = stats.FirstOrDefault(s => s.type == statType);
            if (stat != null)
            {
                stat.value = value;
            }
            else
            {
                stats.Add(new StatInfo(statType, value));
            }
        }
    }
} 