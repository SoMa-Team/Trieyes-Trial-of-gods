using System.Collections.Generic;
using Utils; // For StatInfo
using System.Linq; // For LINQ operations like ToList

namespace CardSystem
{
    public static class StatCalculator
    {
        /// <summary>
        /// 두 StatInfo 리스트를 합산하여 새로운 StatInfo 리스트를 반환합니다.
        /// 동일한 이름의 스탯은 값을 합산하고, 새로운 스탯은 추가합니다.
        /// </summary>
        /// <param name="baseStats">기준 스탯 리스트</param>
        /// <param name="additionalStats">추가할 스탯 리스트</param>
        /// <returns>합산된 StatInfo 리스트</returns>
        public static List<StatInfo> AddStats(List<StatInfo> baseStats, List<StatInfo> additionalStats)
        {
            // Dictionary를 사용하여 스탯 이름별로 StatInfo를 관리하여 빠른 검색 및 업데이트를 가능하게 합니다.
            Dictionary<StatType, StatInfo> combinedStats = new Dictionary<StatType, StatInfo>();

            // 기준 스탯을 딕셔너리에 추가
            foreach (var stat in baseStats)
            {
                if (stat != null)
                {
                    combinedStats[stat.type] = new StatInfo(stat.type, stat.value);
                }
            }

            // 추가 스탯을 합산 또는 새로 추가
            foreach (var stat in additionalStats)
            {
                if (stat != null)
                {
                    if (combinedStats.ContainsKey(stat.type))
                    {
                        // 기존 스탯이 있으면 값 합산
                        combinedStats[stat.type].value += stat.value;
                    }
                    else
                    {
                        // 새로운 스탯이면 추가
                        combinedStats.Add(stat.type, new StatInfo(stat.type, stat.value));
                    }
                }
            }

            // 딕셔너리의 값을 리스트로 변환하여 반환
            return combinedStats.Values.ToList();
        }

        /// <summary>
        /// 단일 StatInfo 객체의 값을 다른 StatInfo 객체에 합산하여 새로운 StatInfo 객체를 반환합니다.
        /// (이것은 다이어그램의 AddStat(Stat, Stat)에 더 가깝습니다. 이름이 다르면 합산하지 않고 baseStat을 반환합니다.)
        /// </summary>
        /// <param name="baseStat">기준 스탯</param>
        /// <param name="additionalStat">추가할 스탯</param>
        /// <returns>합산된 새로운 StatInfo 객체 또는 이름이 다를 경우 baseStat</returns>
        public static StatInfo AddSingleStat(StatInfo baseStat, StatInfo additionalStat)
        {
            if (baseStat == null) return additionalStat; // baseStat이 없으면 additionalStat 반환
            if (additionalStat == null) return baseStat; // additionalStat이 없으면 baseStat 반환

            if (baseStat.type == additionalStat.type)
            {
                // 이름이 같으면 값 합산 후 새로운 StatInfo 반환
                return new StatInfo(baseStat.type, baseStat.value + additionalStat.value);
            }
            else
            {
                // 이름이 다르면 합산할 수 없으므로 baseStat을 그대로 반환
                return baseStat;
            }
        }
    }
} 