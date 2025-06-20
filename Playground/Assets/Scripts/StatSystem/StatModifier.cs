using System;
using Managers;

namespace Stats
{
    /// <summary>
    /// 버프의 연산 타입을 정의합니다.
    /// </summary>
    public enum BuffOperationType
    {
        Additive,           // 합연산 (정수형 수치 증가)
        Multiplicative,     // 곱연산 (퍼센트형 증감)
        Set                 // 특정 값으로 고정
    }

    /// <summary>
    /// 스탯에 적용되는 버프 정보를 저장하는 구조체입니다.
    /// </summary>
    public struct StatModifier
    {
        // --- 필드 ---
        private static int modifierID = 1;
        public int id;                          // 버프 고유 키
        public int value;                           // 버프로 인한 수치 변화
        public BuffOperationType operationType;     // 버프 연산 타입
        public float endTime;                       // 버프 만료 시각
        public bool isPermanent;                    // 영구 버프 여부   
        public bool canStack;                       // 버프 중복 적용 가능 여부

        // --- 생성자 ---

        /// <summary>
        /// StatBuff의 새 인스턴스를 초기화합니다.
        /// duration < 0이면 영구 버프로 간주합니다.
        /// </summary>
        public StatModifier(int buffValue, BuffOperationType operationType, bool canStack=true, float duration = -1f)
        {
            if(duration == 0f){
                throw new ArgumentException("Duration cannot be 0. Use -1 for permanent buff.");
            }
            this.id = modifierID++;
            this.value = buffValue;
            this.operationType = operationType;
            this.canStack = canStack;
            this.isPermanent = duration < 0f;
            if(GameManager.instance==null)
                this.endTime=duration;
            else if (this.isPermanent)
                this.endTime = float.MaxValue;
            else
                this.endTime = GameManager.instance.gameTime + duration;
        }

        // --- 메서드 ---

        /// <summary>
        /// 버프가 만료되었는지 확인합니다.
        /// </summary>
        public bool IsExpired()
        {
            return !isPermanent && GameManager.instance.gameTime >= endTime;
        }
    }
}
