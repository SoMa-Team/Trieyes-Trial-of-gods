namespace RelicSystem
{
    public enum RelicStatType
    {
        DamageIncreasement, // 최종 데미지 증가
        AOE,                // 광역 공격 범위
        Range,              // 투사체 범위
        ProjectileCount,    // 투사체 개수
        ProjectilePierce,   // 투사체 관통 개수
        ProjectileSpeed,    // 투사체 속도
        ProjectileSpread,   // 투사체 분산각도
        SkillCooldownReduction, // 스킬 쿨타임 감소
        Revival,            // 부활
    }
}