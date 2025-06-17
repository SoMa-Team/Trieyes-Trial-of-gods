using UnityEngine;
using CombatSystem;

namespace Stats
{
    /// <summary>
    /// 버프의 연산 타입을 정의하는 enum
    /// </summary>
    public enum BuffOperationType
    {
        Additive,    // 합연산 (수치 증가)
        Multiplicative // 곱연산 (퍼센테이지 증가)
    }

    /// <summary>
    /// 스탯에 적용되는 버프 정보를 저장하는 구조체
    /// </summary>
    public struct StatBuff
    {
        public string buffName;        // 버프 이름
        public float value;           // 버프로 인한 수치 변화
        public BuffOperationType operationType; // 버프 연산 타입
        public float endTime;         // 버프가 끝나는 시간
        public bool isPermanent;      // 영구 버프 여부

        public StatBuff(string name, float buffValue, BuffOperationType operationType, float duration = -1f)
        {
            buffName = name;
            value = buffValue;
            this.operationType = operationType;
            isPermanent = duration < 0f;
            endTime = isPermanent ? float.MaxValue : CombatStageManager.Instance.GetTime() + duration;
        }

        /// <summary>
        /// 버프가 만료되었는지 확인합니다.
        /// </summary>
        public bool IsExpired()
        {
            return !isPermanent && CombatStageManager.Instance.GetTime() >= endTime;
        }
    }
} 