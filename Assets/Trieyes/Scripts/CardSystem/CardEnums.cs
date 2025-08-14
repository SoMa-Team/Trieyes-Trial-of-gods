namespace CardSystem
{
    // ===== [기능 1] Rarity Enum =====
    /// <summary>
    /// 카드의 희귀도를 나타내는 열거형입니다.
    /// 카드의 등급과 강화 가능성을 결정하는 데 사용됩니다.
    /// </summary>
    public enum Rarity
    {
        Common = 0,

        Uncommon = 1,

        Legendary = 2,
        
        Exceed = 3,
    }

    // ===== [기능 2] Property Enum =====
    /// <summary>
    /// 카드가 가질 수 있는 속성들을 정의하는 열거형입니다.
    /// 각 속성은 카드의 효과와 스탯에 영향을 미칩니다.
    /// </summary>
    public enum Property
    {
        Fire = 0,
        Wind = 1,
        Light = 2,
        Dark = 3,
        Steel = 4,
    }
} 