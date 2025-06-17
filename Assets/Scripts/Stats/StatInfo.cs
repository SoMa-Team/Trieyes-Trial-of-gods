namespace Stats
{
    /// <summary>
    /// 스탯 정보를 담는 클래스
    /// </summary>
    public class StatInfo
    {
        // ===== [기능 1] 스탯 정보 및 생성 =====
        public StatType Type;
        public float Value;

        public StatInfo(StatType type, float value)
        {
            this.Type = type;
            this.Value = value;
        }

        public override string ToString()
        {
            return $"{Type}: {Value}";
        }
    }
} 