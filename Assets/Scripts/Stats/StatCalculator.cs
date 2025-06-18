using System.Collections.Generic;
using System.Linq;

namespace Stats
{
    // 스탯 계산 유틸리티 클래스
    public static class StatCalculator
    {
        // ===== [기능 1] 스탯 합산 =====
        /// <summary>
        /// 두 스탯 리스트를 합산합니다.
        /// </summary>
        /// <param name="baseStats">기본 스탯 리스트</param>
        /// <param name="additionalStats">추가할 스탯 리스트</param>
        /// <returns>합산된 스탯 리스트</returns>
        public static List<StatInfo> AddStats(List<StatInfo> baseStats, List<StatInfo> additionalStats)
        {
            Dictionary<StatType, StatInfo> combinedStats = new();
            foreach (var stat in baseStats)
            {
                combinedStats[stat.Type] = new StatInfo(stat.Type, stat.Value);
            }
            foreach (var stat in additionalStats)
            {
                if (combinedStats.ContainsKey(stat.Type))
                {
                    combinedStats[stat.Type].Value += stat.Value;
                }
                else
                {
                    combinedStats[stat.Type] = new StatInfo(stat.Type, stat.Value);
                }
            }
            return combinedStats.Values.ToList();
        }

        // ===== [기능 2] 스탯 값 조회 =====
        /// <summary>
        /// 스탯 리스트에서 특정 타입의 스탯 값을 가져옵니다.
        /// </summary>
        /// <param name="stats">스탯 리스트</param>
        /// <param name="statType">찾을 스탯 타입</param>
        /// <returns>스탯 값, 없으면 0</returns>
        public static float GetStatValue(List<StatInfo> stats, StatType statType)
        {
            var stat = stats.FirstOrDefault(s => s.Type == statType);
            return stat?.Value ?? 0f;
        }

        // ===== [기능 3] 스탯 값 설정 =====
        /// <summary>
        /// 스탯 리스트에서 특정 타입의 스탯을 설정합니다.
        /// </summary>
        /// <param name="stats">스탯 리스트</param>
        /// <param name="statType">설정할 스탯 타입</param>
        /// <param name="value">설정할 값</param>
        public static void SetStatValue(List<StatInfo> stats, StatType statType, float value)
        {
            var stat = stats.FirstOrDefault(s => s.Type == statType);
            if (stat != null)
            {
                stat.Value = value;
            }
            else
            {
                stats.Add(new StatInfo(statType, value));
            }
        }
    }
} 