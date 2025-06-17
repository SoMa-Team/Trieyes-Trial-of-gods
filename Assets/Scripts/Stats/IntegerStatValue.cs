using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stats
{
    /// <summary>
    /// 정수형 스탯 값을 관리하는 구조체 또는 클래스입니다.
    /// </summary>
    public class IntegerStatValue
    {
        //기본 스탯 값
        private int Basicvalue;
        //현재 스탯 값
        private int Currentvalue;
        // 최대 스탯 값 (선택 사항)
        public int? maxValue;
        // 최소 스탯 값 (선택 사항)
        public int? minValue;

        // 현재 적용된 버프 리스트
        private StatBuffList activeBuffs = new StatBuffList();

        /// <summary>
        /// IntegerStatValue의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="baseValue">기본 스탯 값</param>
        /// <param name="max">최대 스탯 값 (선택 사항)</param>
        /// <param name="min">최소 스탯 값 (선택 사항)</param>
        public IntegerStatValue(int baseValue, int? max = null, int? min = null)
        {
            value = baseValue;
            maxValue = max;
            minValue = min;
        }

        /// <summary>
        /// 스탯 값을 변경하고 최대/최소 값 범위 내로 유지합니다.
        /// </summary>
        /// <param name="amount">변경할 양</param>
        public void Add(int amount)
        {
            value += amount;
            ApplyMinMax();
        }

        /// <summary>
        /// 스탯 값을 함수를 적용하여 변경하고 최대/최소 값 범위 내로 유지합니다.
        /// </summary>
        /// <param name="modifier">스탯 값을 변경할 함수</param>
        public void Add(System.Func<int, int> modifier)
        {
            value = modifier(value);
            ApplyMinMax();
        }

        /// <summary>
        /// 스탯 값을 변경 후 최대/최소 값 범위 내로 조정합니다.
        /// </summary>
        private void ApplyMinMax()
        {
            if (maxValue.HasValue)
            {
                value = Mathf.Min(value, maxValue.Value);
            }
            if (minValue.HasValue)
            {
                value = Mathf.Max(value, minValue.Value);
            }
        }

        /// <summary>
        /// 스탯 값을 직접 설정합니다.
        /// </summary>
        /// <param name="newValue">설정할 새 값</param>
        public void Set(int newValue)
        {
            value = newValue;
            ApplyMinMax();
        }

        /// <summary>
        /// 새로운 버프를 추가합니다.
        /// </summary>
        /// <param name="buff">추가할 버프 정보</param>
        public void AddBuff(StatBuff buff)
        {
            activeBuffs.Add(buff);
            RecalculateValue();
        }

        /// <summary>
        /// 모든 버프를 제거합니다.
        /// </summary>
        public void ClearBuffs()
        {
            activeBuffs.Clear();
            RecalculateValue();
        }

        /// <summary>
        /// 버프의 지속 시간을 체크하고 만료된 버프를 제거합니다.
        /// </summary>
        public void UpdateBuffs()
        {
            RecalculateValue();
        }

        /// <summary>
        /// 기본값과 모든 버프를 고려하여 최종 값을 재계산합니다.
        /// </summary>
        private void RecalculateValue()
        {
            value = activeBuffs.CalculateBuff(Basicvalue);
            ApplyMinMax();
        }

        // 암시적 int 변환 연산자 오버로드
        public static implicit operator int(IntegerStatValue stat) => stat.value;
    }
} 