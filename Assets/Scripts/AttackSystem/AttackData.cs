namespace AttackSystem
{
    // ===== [기능 1] AttackData 정보 =====
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

    // ===== [기능 2] AttackType Enum =====
    public enum AttackType
    {
        Basic,
        Skill,
        // 기타 타입
    }
} 