using System.Collections.Generic;
using CombatSystem;

namespace Stats
{
    /// <summary>
    /// StatBuff를 순서대로 관리하고 계산하는 클래스
    /// </summary>
    public class StatBuffList
    {
        private List<StatBuff> buffs = new List<StatBuff>();

        /// <summary>
        /// 버프를 리스트에 추가합니다.
        /// </summary>
        public void Add(StatBuff buff)
        {
            buffs.Add(buff);
        }

        /// <summary>
        /// 모든 버프를 제거합니다.
        /// </summary>
        public void Clear()
        {
            buffs.Clear();
        }

        /// <summary>
        /// 기본값에 모든 유효한 버프를 적용하여 최종 값을 계산합니다.
        /// 계산 전에 만료된 버프를 자동으로 제거합니다.
        /// </summary>
        /// <param name="basicValue">기본 스탯 값</param>
        /// <returns>버프가 적용된 최종 스탯 값</returns>
        public int CalculateBuff(int basicValue)
        {
            // 만료된 버프 제거
            float currentTime = CombatStageManager.Instance.GetTime();
            buffs.RemoveAll(buff => !buff.isPermanent && buff.endTime <= currentTime);

            float additiveValue = 0f;
            float multiplicativeValue = 1f;

            foreach (var buff in buffs)
            {
                if (buff.operationType == BuffOperationType.Additive)
                {
                    additiveValue += buff.value;
                }
                else // Multiplicative
                {
                    multiplicativeValue *= (1f + buff.value);
                }
            }

            // 최종 계산: (기본값 + 합연산 버프) * 곱연산 버프
            return UnityEngine.Mathf.RoundToInt((basicValue + additiveValue) * multiplicativeValue);
        }

        /// <summary>
        /// 현재 적용된 버프의 개수를 반환합니다.
        /// </summary>
        public int Count()
        {
            return buffs.Count;
        }
    }
} 