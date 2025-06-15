using System;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// 정수형 스탯 값을 관리하는 구조체 또는 클래스입니다.
    /// </summary>
    public class IntegerStatValue
    {
        // 현재 스탯 값
        public int value;

        // 최대 스탯 값 (선택 사항)
        public int? maxValue;

        // 최소 스탯 값 (선택 사항)
        public int? minValue;

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

        // 암시적 int 변환 연산자 오버로드
        public static implicit operator int(IntegerStatValue stat) => stat.value;
    }
} 