using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Stats;
using TagSystem;
using UnityEngine.Rendering.Universal.Internal;

namespace AttackSystem
{
    // ===== [기능 1] AttackData 정보 =====
    [CreateAssetMenu(fileName = "AttackData", menuName = "Attack/AttackData", order = 1)]
    public class AttackData : ScriptableObject
    {
        public int attackId;
        public string attackName;
        public AttackType attackType;
        public List<AttackTag> tags;
        public float cooldown; // Type이 Basic일 경우, 값은 무시됨.
        public float damageMultiplier = 1;
        public string attackIcon;

        public AttackData Copy()
        {
            var copy = new AttackData();
            copy.attackId = attackId;
            copy.attackName = attackName;
            copy.attackType = attackType;
            copy.tags = tags.ToList();
            copy.cooldown = cooldown;
            copy.damageMultiplier = damageMultiplier;
            copy.attackIcon = attackIcon;
            return copy;
        }
    }

    // ===== [기능 2] AttackType Enum =====
    public enum AttackType
    {
        Basic,
        Skill,
        // 기타 타입
    }
} 