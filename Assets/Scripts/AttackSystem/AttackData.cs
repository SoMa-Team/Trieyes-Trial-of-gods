using UnityEngine;
using Stats;

namespace AttackSystem
{
    // ===== [기능 1] AttackData 정보 =====
    [CreateAssetMenu(fileName = "AttackData", menuName = "Attack/AttackData", order = 1)]
    public class AttackData : ScriptableObject
    {
        public int attackId;
        public string attackName;
        public AttackType attackType;
        public float cooldown; // Type이 Basic일 경우, 값은 무시됨.
        public float damageMultiplier = 1;
        public string attackIcon;
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