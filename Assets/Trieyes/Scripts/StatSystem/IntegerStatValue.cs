using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using BattleSystem;

namespace Stats
{
    /// 정수형 스탯 값을 관리하는 클래스입니다.
    /// 외부에서는 Value 프로퍼티만 사용하세요.
    public class IntegerStatValue
    {
        // --- 필드 ---

        //기본 스탯 값
        private int basicValue;
        //현재 스탯 값
        private int currentValue;
        // 최대 스탯 값 (선택 사항)
        public int? maxValue;
        // 최소 스탯 값 (선택 사항)
        public int? minValue;
        // 버프 리스트에 변동이 있는지 확인
        private bool modifierListChanged;
        // 기본 값이 변경되었는지 확인
        private bool basicValueChanged;

        // 현재 적용된 버프 리스트
        private StatModifierList activeModifiers = new StatModifierList();
        // 버프 힙
        private MinHeap<float> modifierHeap = new MinHeap<float>();

        // --- 생성자 ---

        /// IntegerStatValue의 새 인스턴스를 초기화합니다.
        public IntegerStatValue(int initialValue, int? max = null, int? min = null)
        {
            // Debug.Log($"initialValue: {initialValue}");
            basicValue = initialValue;
            maxValue = max;
            minValue = min;
            modifierListChanged = false;
            basicValueChanged = true;
        }

        // --- 프로퍼티 ---

        // 암시적 int 변환 연산자 오버로드
        public static implicit operator int(IntegerStatValue stat) => stat.Value;

        // --- public 메서드 ---

        /// 스탯 값을 변경하고 최대/최소 값 범위 내로 유지합니다.
        public void AddToBasicValue(int amount)
        {
            basicValue += amount;
            basicValueChanged = true;
        }
        /// 스탯 값을 함수를 적용하여 변경하고 최대/최소 값 범위 내로 유지합니다.
        public void MultiplyToBasicValue(int factor)
        {
            basicValue *= factor;
            basicValueChanged = true;
        }
        /// 스탯 값을 직접 설정합니다.
        public void SetBasicValue(float newValue)
        {
            basicValue = (int)newValue;
            basicValueChanged = true;
        }
         /// 새로운 버프를 추가합니다.
        public void AddBuff(StatModifier modifier)
        {
            activeModifiers.Add(modifier);
            if(!modifier.isPermanent) modifierHeap.Push(modifier.endTime);
            modifierListChanged = true;
            CardStatChangeRecorder.Instance.AddStatValueTrigger(modifier);
        }
        /// 모든 버프를 제거합니다.
        public void ClearBuffs()
        {
            activeModifiers.Clear();
            modifierHeap.Clear();
            modifierListChanged = true;
        }
        //외부에서 값을 읽을 때는 항상 GetCurrentValue()가 호출되도록
        public int Value => GetCurrentValue();

        // --- private 메서드 ---

        /// 스탯 값을 변경 후 최대/최소 값 범위 내로 조정합니다.
        private void ApplyMinMax()
        {
            if (maxValue.HasValue)
            {
                currentValue = Mathf.Min(currentValue, maxValue.Value);
            }
            if (minValue.HasValue)
            {
                currentValue = Mathf.Max(currentValue, minValue.Value);
            }
        }

        /// 기본값과 모든 버프를 고려하여 최종 값을 재계산합니다.
        private void RecalculateValue()
        {
            currentValue = activeModifiers.CalculateBuff(basicValue);
            ApplyMinMax();
        }

        private int GetCurrentValue()
        {
            float currentTime;

            // TODO: now 검증이 필요한가?
            if (BattleStage.now == null)
            {
                currentTime = 0;
            }
            else currentTime = BattleStage.now.GetTime();
            
            // if(!modifierHeap.IsEmpty)Debug.Log($"currentTime: {currentTime}, {modifierHeap.Peek()}");
            // else Debug.Log($"currentTime: {currentTime}, modifierHeap.IsEmpty");

            // 만료된 버프를 모두 제거
            while (!modifierHeap.IsEmpty && modifierHeap.Peek() < currentTime)
            {
                modifierHeap.Pop();
                modifierListChanged = true;
            }

            // 버프 리스트에 변동이 생겼거나, 기본 값이 변경되었으면 재계산
            if (modifierListChanged||basicValueChanged)
            {
                RecalculateValue();
                modifierListChanged = false;
                basicValueChanged = false;
            }
            return currentValue;
        }
        
        public IntegerStatValue DeepCopy()
        {
            var copy = new IntegerStatValue(this.basicValue, this.maxValue, this.minValue);
            copy.currentValue = this.currentValue;
            copy.modifierListChanged = this.modifierListChanged;
            copy.basicValueChanged = this.basicValueChanged;
            copy.activeModifiers = this.activeModifiers.DeepCopy();
            copy.modifierHeap = this.modifierHeap.DeepCopy();
            return copy;
        }
    }
} 
