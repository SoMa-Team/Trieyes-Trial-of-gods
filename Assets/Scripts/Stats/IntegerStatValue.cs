using System;
using UnityEngine;

namespace Stats
{
    /// <summary>
    /// 정수형 스탯 값을 관리하는 구조체 또는 클래스입니다.
    /// </summary>
    public class IntegerStatValue
    {
        // ===== [기능 1] 값 및 생성자 =====
        public int value;
        public int? maxValue;
        public int? minValue;
        public IntegerStatValue(int baseValue, int? max = null, int? min = null)
        {
            value = baseValue;
            maxValue = max;
            minValue = min;
        }
        // ===== [기능 2] 값 변경 및 보정 =====
        public void Add(int amount)
        {
            value += amount;
            ApplyMinMax();
        }
        public void Add(System.Func<int, int> modifier)
        {
            value = modifier(value);
            ApplyMinMax();
        }
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
        public void Set(int newValue)
        {
            value = newValue;
            ApplyMinMax();
        }
        public static implicit operator int(IntegerStatValue stat) => stat.value;
    }
} 