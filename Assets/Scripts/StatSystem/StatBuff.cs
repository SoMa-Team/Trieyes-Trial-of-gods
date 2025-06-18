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
        Multiplicative, // 곱연산 (퍼센테이지 증가)
        Set // 설정 (수치 설정)
    }

    /// <summary>
    /// 스탯에 적용되는 버프 정보를 저장하는 구조체
    /// </summary>
    public struct StatBuff
    {
        public int value;           // 버프로 인한 수치 변화
        public BuffOperationType operationType; // 버프 연산 타입
        public float endTime;         // 버프가 끝나는 시간
        public bool isPermanent;      // 영구 버프 여부
        //duration < 0이면 해당 스테이지에서 영구 적용되는 버프(후에 전투 진입 전 카드 버프 적용 시 등)
        public StatBuff(int buffValue, BuffOperationType operationType, float duration = -1f)
        {
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