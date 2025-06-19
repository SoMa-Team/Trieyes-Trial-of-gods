using UnityEngine;
using UnityEngine.UIElements.Experimental;

[System.Serializable]
public class AttackStats
{
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;

    public float projectileSpeed;
    public int projectileCount;
    public int projectilePierce;

    public float criticalChance;
    public float criticalDamage;

    public float lifesteal;

    public float bossDamageBonus;
    public float armorPenetration;
}


[System.Serializable]
public class DefenseStats
{
    public float defense;
    public float reflectDamage;
}

[System.Serializable]
public class VitalStats
{
    public float maxHealth;
    public float maxMana;

    public float healthRegen;
    public float manaRegen;
}

[System.Serializable]
public class UtilityStats
{
    public float moveSpeed;
    public float magnetRange;

    public float rewardQuantityBonus;
    public float rewardQualityBonus;

    public float skillCooldownReduction;

    public float spawnQuantityMultiplier;
    public float spawnQualityMultiplier;
}
[System.Serializable]
public class StatManager
{
    public AttackStats attakStats;
    public DefenseStats defenseStats;
    public VitalStats vitalStats;
    public UtilityStats utilityStats;
}
