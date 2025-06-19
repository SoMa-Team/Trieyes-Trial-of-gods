namespace Stats
{
    /// <summary>
    /// 게임 내 스탯 타입을 정의하는 enum입니다.
    /// </summary>
    public enum StatType
    {
        // 공격 관련 스탯
        AttackPower,        // 공격력
        AttackSpeed,        // 공격속도
        AttackRange,        // 공격 범위
        ProjectileCount,    // 투사체 개수
        ProjectilePierce,   // 투사체 관통 개수
        CriticalRate,       // 치명타 확률
        CriticalDamage,     // 치명타 데미지
        LifeSteal,          // 흡혈 계수
        BossDamageMultiplier, // 보스 추가 공격 배율

        // 방어 관련 스탯
        Defense,            // 방어력
        DefensePenetration, // 방어 관통력
        Reflect,            // 반사

        // 생명 관련 스탯
        Health,             // 체력
        HealthRegen,        // 자연 회복

        // 유틸리티 스탯
        MoveSpeed,          // 이동속도
        ItemMagnet,         // 자기력 (아이템 획득 반경)
        GoldDropRate,       // 골드 드랍율
        ShopQualityMultiplier, // 상점 보상 질적 배수
        SkillCooldownReduction, // 스킬 쿨타임 감소
        RewardQuantityMultiplier, // 보상 양적 배수
        RewardQualityMultiplier,  // 보상 질적 배수

        // 효과 관련 스탯 (MVP 구현 X)
        Revival,            // 부활
        InstantDeath        // 즉사
    }
} 