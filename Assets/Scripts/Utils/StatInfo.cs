using UnityEngine;

namespace Utils
{
    /// <summary>
    /// 게임 내 스탯 정보를 담는 클래스입니다.
    /// </summary>
    public class StatInfo
    {
        public StatType type;  // 스탯 타입
        public float value;    // 스탯 값

        public StatInfo(StatType type, float value)
        {
            this.type = type;
            this.value = value;
        }

        public override string ToString()
        {
            return $"{type}: {value}";
        }
    }
} 