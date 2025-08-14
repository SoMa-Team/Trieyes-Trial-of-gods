using System.Collections.Generic;
using BattleSystem;
using System;
using UnityEngine;

namespace Stats
{
    /// <summary>
    /// StatBuff를 순서대로 관리하고 계산하는 클래스입니다.
    /// </summary>
    public class StatModifierList
    {
        // --- 필드 ---

        private List<StatModifier> modifiers = new List<StatModifier>();

        // --- 메서드 ---

        /// <summary>
        /// 버프를 리스트에 추가합니다.
        /// </summary>
        public void Add(StatModifier modifier)
        {
            if(!modifier.canStack){
                modifiers.RemoveAll(b => b.id == modifier.id);
            }
            modifiers.Add(modifier);
        }

        /// <summary>
        /// 모든 버프를 제거합니다.
        /// </summary>
        public void Clear()
        {
            modifiers.Clear();
        }

        /// <summary>
        /// 기본값에 모든 유효한 버프를 적용하여 최종 값을 계산합니다.
        /// 계산 전에 만료된 버프를 자동으로 제거합니다.
        /// </summary>
        public int CalculateBuff(int basicValue)
        {
            float currentTime;
            if(BattleStage.now == null)
            {
                currentTime = 0f;
            }
            // 만료된 버프 제거
            else currentTime = BattleStage.now.GetTime();
            //Debug.Log($"currentTime: {currentTime}");
            modifiers.RemoveAll(buff => !buff.isPermanent && buff.endTime <= currentTime);

            int finalValue = basicValue;

            foreach (var buff in modifiers)
            {
                if(BattleStage.now is not null) Debug.Log($"currentTime: {BattleStage.now.GetTime()} | buff.endTime: {buff.endTime}");
                finalValue = buff.getNextValue(finalValue);
            }

            return finalValue;
        }

        /// <summary>
        /// 현재 적용된 버프의 개수를 반환합니다.
        /// </summary>
        public int Count()
        {
            return modifiers.Count;
        }
        
        public StatModifierList DeepCopy()
        {
            var copy = new StatModifierList();
            foreach (var modifier in this.modifiers)
            {
                copy.Add(modifier);
            }
            return copy;
        }
    }
}
