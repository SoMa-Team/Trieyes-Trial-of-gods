using System;
using System.Collections.Generic;
using Stats;
using UnityEngine;

namespace Utils
{
    public static class StatTypeTransformer
    {
        // StatType → 한글
        private static readonly Dictionary<StatType, string> StatTypeToKor = new()
        {
            { StatType.AttackPower, "공격력" },
            { StatType.AttackSpeed, "공격속도" },
            { StatType.AttackRange, "사정거리" },
            { StatType.ProjectileCount, "투사체 개수" },
            { StatType.ProjectilePierce, "투사체 관통 개수" },
            { StatType.ProjectileSpeed, "투사체 속도" },
            { StatType.ProjectileSpread, "투사체 분산각도" },
            { StatType.CriticalRate, "치명타 확률" },
            { StatType.CriticalDamage, "치명타 데미지" },
            { StatType.LifeSteal, "흡혈 계수" },
            { StatType.BossDamageMultiplier, "보스 추가 공격 배율" },

            { StatType.Defense, "방어력" },
            { StatType.DefensePenetration, "방어 관통력" },
            { StatType.Evasion, "회피율" },
            { StatType.Reflect, "반사" },

            { StatType.Health, "체력" },
            { StatType.HealthRegen, "자연 회복" },

            { StatType.MoveSpeed, "이동속도" },
            { StatType.ItemMagnet, "자기력" },
            { StatType.GoldDropRate, "골드 드랍율" },
            { StatType.ShopQualityMultiplier, "상점 보상 질적 배수" },
            { StatType.SkillCooldownReduction, "스킬 쿨타임 감소" },
            { StatType.RewardQuantityMultiplier, "보상 양적 배수" },
            { StatType.RewardQualityMultiplier, "보상 질적 배수" },

            { StatType.Revival, "부활" },
            { StatType.InstantDeath, "즉사" }
        };

        // 한글 → StatType
        private static readonly Dictionary<string, StatType> KorToStatType = new()
        {
            { "공격력", StatType.AttackPower },
            { "공격속도", StatType.AttackSpeed },
            { "사정거리", StatType.AttackRange },
            { "투사체 개수", StatType.ProjectileCount },
            { "투사체 관통 개수", StatType.ProjectilePierce },
            { "투사체 속도", StatType.ProjectileSpeed },
            { "투사체 분산각도", StatType.ProjectileSpread },
            { "치명타 확률", StatType.CriticalRate },
            { "치명타 데미지", StatType.CriticalDamage },
            { "흡혈 계수", StatType.LifeSteal },
            { "보스 추가 공격 배율", StatType.BossDamageMultiplier },

            { "방어력", StatType.Defense },
            { "방어 관통력", StatType.DefensePenetration },
            { "회피율", StatType.Evasion },
            { "반사", StatType.Reflect },

            { "체력", StatType.Health },
            { "자연 회복", StatType.HealthRegen },

            { "이동속도", StatType.MoveSpeed },
            { "자기력", StatType.ItemMagnet },
            { "골드 드랍율", StatType.GoldDropRate },
            { "상점 보상 질적 배수", StatType.ShopQualityMultiplier },
            { "스킬 쿨타임 감소", StatType.SkillCooldownReduction },
            { "보상 양적 배수", StatType.RewardQuantityMultiplier },
            { "보상 질적 배수", StatType.RewardQualityMultiplier },

            { "부활", StatType.Revival },
            { "즉사", StatType.InstantDeath }
        };

        // 영문 문자열 → StatType (enum 이름과 매칭)
        private static readonly Dictionary<string, StatType> StringToStatType = new(StringComparer.OrdinalIgnoreCase);

        static StatTypeTransformer()
        {
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                StringToStatType[stat.ToString()] = stat;
            }
        }

        /// <summary>
        /// StatType → 한글
        /// </summary>
        public static string StatTypeToKorean(StatType statType)
        {
            if (StatTypeToKor.TryGetValue(statType, out var kor))
                return kor;
            throw new InvalidOperationException($"[StatTypeTransformer] StatType을 한글로 변환할 수 없습니다: {statType}");
        }

        /// <summary>
        /// 한글 → StatType
        /// </summary>
        public static StatType KoreanToStatType(string korean)
        {
            if (KorToStatType.TryGetValue(korean.Trim(), out var statType))
                return statType;
            throw new InvalidOperationException($"[StatTypeTransformer] 알 수 없는 한글 스탯명: '{korean}'");
        }

        /// <summary>
        /// 영문(enum 이름) → StatType
        /// </summary>
        public static StatType ParseStatType(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new InvalidOperationException("[StatTypeTransformer] 입력값이 null 또는 공백입니다.");

            if (StringToStatType.TryGetValue(input.Trim(), out var statType))
                return statType;

            throw new InvalidOperationException($"[StatTypeTransformer] 영문 스탯 파싱 실패: '{input}'");
        }
    }
}
