namespace AttackSystem
{
    public class AttackData
    {
        public int attackId;
        public string attackName;
        public string attackIcon;
        public AttackType attackType;
        public float damage;
        public float cooldown;
        public bool bIsActivated;
        // 기타 공격 관련 정보
    }

    public enum AttackType
    {
        Basic,
        Skill,
        // 기타 타입
    }
} 