using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stats
{
    /// 정수형 스탯 값을 관리하는 구조체 또는 클래스입니다.
    public class IntegerStatValue
    {
        //기본 스탯 값
        private int basicValue;
        //현재 스탯 값
        private int currentValue;
        // 최대 스탯 값 (선택 사항)
        public int? maxValue;
        // 최소 스탯 값 (선택 사항)
        public int? minValue;
        // 마지막 버프 리스트 크기
        private int lastListSize;
        // 기본 값이 변경되었는지 확인
        private bool basicValueChanged;

        // 현재 적용된 버프 리스트
        private StatBuffList activeBuffs = new StatBuffList();
        // 버프 힙
        private StatBuffHeap buffHeap = new StatBuffHeap();

        /// IntegerStatValue의 새 인스턴스를 초기화합니다.
        public IntegerStatValue(int baseValue, int? max = null, int? min = null)
        {
            basicValue = baseValue;
            maxValue = max;
            minValue = min;
            lastListSize = 0;
            basicValueChanged = true;
        }

        /// 스탯 값을 변경하고 최대/최소 값 범위 내로 유지합니다.
        public void Add(int amount)
        {
            basicValue += amount;
            basicValueChanged = true;
        }
        /// 스탯 값을 함수를 적용하여 변경하고 최대/최소 값 범위 내로 유지합니다.
        public void Multiply(int factor)
        {
            basicValue *= factor;
            basicValueChanged = true;
        }

        /// 스탯 값을 변경 후 최대/최소 값 범위 내로 조정합니다.
        private void ApplyMinMax()
        {
            if (maxValue.HasValue)
            {
                basicValue = Mathf.Min(basicValue, maxValue.Value);
                currentValue = Mathf.Min(currentValue, maxValue.Value);
            }
            if (minValue.HasValue)
            {
                basicValue = Mathf.Max(basicValue, minValue.Value);
                currentValue = Mathf.Max(currentValue, minValue.Value);
            }
        }

        /// 스탯 값을 직접 설정합니다.
        public void Set(int newValue)
        {
            basicValue = newValue;
            basicValueChanged = true;
        }

        /// 새로운 버프를 추가합니다.
        public void AddBuff(StatBuff buff)
        {
            activeBuffs.Add(buff);
            buffHeap.Push(buff);
        }

        /// 모든 버프를 제거합니다.
        public void ClearBuffs()
        {
            activeBuffs.Clear();
            buffHeap.Clear();
        }

        /// 기본값과 모든 버프를 고려하여 최종 값을 재계산합니다.
        private void RecalculateValue()
        {
            currentValue = activeBuffs.CalculateBuff(basicValue);
            ApplyMinMax();
        }

        public int GetCurrentValue(){
            float currentTime = CombatStageManager.Instance.GetTime();
            bool removedAny = false;

            // 만료된 버프를 모두 제거
            var top = buffHeap.Peek();
            while (top != null && top.endTime < currentTime)
            {
                buffHeap.Pop();
                removedAny = true;
                top = buffHeap.Peek();
            }

            // 버프가 하나라도 빠졌으면 재계산
            if (removedAny||lastListSize!=activeBuffs.Count||basicValueChanged)
            {
                RecalculateValue();
                lastListSize = activeBuffs.Count;
                basicValueChanged = false;
            }

            return currentValue;
        }


        public int GetBasicValue()
        {
            return basicValue;
        }

        //외부에서 값을 읽을 때는 항상 GetCurrentValue()가 호출되도록
        public int Value => GetCurrentValue();
        // 암시적 int 변환 연산자 오버로드
        public static implicit operator int(IntegerStatValue stat) => stat.Value;   
    }
} 