using UnityEngine;
using CharacterSystem;
using System;
using System.Collections.Generic;

namespace Stats
{
    // 정규화 계산에 필요한 컨텍스트
    public readonly struct StatEvalCtx
    {
        public readonly Pawn Pawn;          // 필요 시 참조용
        public readonly int Raw;            // 이 스탯의 Raw 최종값 (버프 적용 후 정수)
        public readonly Func<StatType,int> GetRaw; // 다른 스탯 Raw 참조가 필요하면 사용
        public StatEvalCtx(Pawn pawn, int raw, Func<StatType,int> getRaw)
        { Pawn = pawn; Raw = raw; GetRaw = getRaw; }
    }

    public static class StatFormulas
    {
        // 최종값 계산 함수 맵
        private static readonly Dictionary<StatType, Func<StatEvalCtx, float>> statMap
            = new Dictionary<StatType, Func<StatEvalCtx, float>>
        {
            // 1) 그대로 쓰는 항목은 통일된 람다
            { StatType.AttackPower, ctx => ctx.Raw },
            { StatType.CriticalRate, ctx => ctx.Raw },
            { StatType.CriticalDamage, ctx => ctx.Raw },
            { StatType.Reflect, ctx => ctx.Raw },
            { StatType.Health, ctx => ctx.Raw },
            { StatType.Evasion, ctx => ctx.Raw },
            { StatType.ItemMagnet, ctx => ctx.Raw },

            // 2) 정규화/비선형 공식
            { StatType.AttackSpeed, ctx =>
                Mathf.Clamp(ctx.Raw * (1f/100f), 0.01f, 10f)
            },
            { StatType.SkillCooldownReduction, ctx =>
                Mathf.Clamp(1000f / (1000f + ctx.Raw), 0.25f, 1f)
            },
            { StatType.Defense, ctx =>
                Mathf.Clamp(1000f / (1000f + ctx.Raw), 0.10f, 1f)
            },
            { StatType.MoveSpeed, ctx =>
            {
                const float sensitivity = 0.01f;
                float logValue = Mathf.Log(1f + Mathf.Max(ctx.Raw * sensitivity, 0f));
                return Mathf.Clamp(logValue, 0f, 20f);
            }},
            { StatType.HealthRegen, ctx =>
                Mathf.Clamp(ctx.Raw, 0f, 100f)
            },
            { StatType.LifeSteal, ctx =>
                Mathf.Clamp(ctx.Raw * (1f/100f), 0f, 10f)
            },
            { StatType.GoldDropRate, ctx =>
                Mathf.Clamp(ctx.Raw, 0f, 100f)
            },
        };

        public static float Eval(StatType type, StatEvalCtx ctx)
        {
            if (statMap.TryGetValue(type, out var f)) return f(ctx);
            // 등록되지 않은 스탯은 Raw 반환(안전장치)
            return ctx.Raw;
        }
    }
}