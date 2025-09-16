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
        CriticalRate,       // 치명타 확률
        CriticalDamage,     // 치명타 데미지
        LifeSteal,          // 흡혈 계수

        // 방어 관련 스탯
        Defense,            // 방어력
        Evasion,            // 회피율
        Reflect,            // 반사

        // 생명 관련 스탯
        Health,             // 체력
        HealthRegen,        // 자연 회복

        // 유틸리티 스탯
        MoveSpeed,          // 이동속도
        ItemMagnet,         // 자기력 (아이템 획득 반경)
        GoldDropRate,       // 골드 드랍율
        SkillCooldownReduction, // 스킬 쿨타임 감소
        DeckSize,
    }
}