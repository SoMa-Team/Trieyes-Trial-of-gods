namespace Utils
{
    public class StatInfo
    {
        /// 스탯의 이름
        public string name;
        
        /// 스탯의 값
        public IntegerStatValue statValue;
        
        /// StatInfo의 새 인스턴스를 초기화합니다.
        public StatInfo(string name, int baseStat, int? max = null, int? min = null)
        {
            this.name = name;
            statValue = new IntegerStatValue(baseStat, max, min);
        }
    }
} 