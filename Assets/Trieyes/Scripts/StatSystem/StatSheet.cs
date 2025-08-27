using System;
using System.Collections.Generic;
using Utils;
using CharacterSystem;

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

        const int DEFAULT_VALUE = 0;

        private Pawn owner;

        // --- 생성자 ---

        /// StatSheet의 새 인스턴스를 초기화합니다.
        /// 모든 StatType에 대해 기본값(0)으로 초기화됩니다.
        public StatSheet()
        {
            // Enum 전체를 순회하며 초기화 (기본값 필요 시 변경)
            foreach (StatType type in Enum.GetValues(typeof(StatType)))
            {
                stats[type] = new IntegerStatValue(DEFAULT_VALUE);
            }
        }

        // --- 메서드 ---
        public void ClearBuffs()
        {
            foreach (var stat in stats)
            {
                stat.Value.ClearBuffs();
            }
        }
        
        public int GetRaw(StatType type)
        {
            return stats[type].Value;
        }
        
        public float Get(StatType type)
        {
            int raw = stats[type].Value;
            var ctx = new StatEvalCtx(
                raw,
                t => GetRaw(t)
            );
            return StatFormulas.Eval(type, ctx);
        }

        // --- 인덱서 ---

        /// StatType으로 해당 스탯 값을 조회합니다.
        public IntegerStatValue this[StatType type]
        {
            get
            {
                CardStatChangeRecorder.Instance.AddStatTrigger(type);
                return stats[type];
            }

            set => stats[type] = value;
        }


        // DeepCopy Method
        public StatSheet DeepCopy()
        {
            var copiedSheet = new StatSheet();
            foreach (StatType type in Enum.GetValues(typeof(StatType)))
                copiedSheet.stats[type] = stats[type].DeepCopy();
            return copiedSheet;
        }
    }
}
