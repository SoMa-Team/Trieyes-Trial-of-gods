namespace CardSystem
{
    // ===== [기능 1] Rarity Enum =====
    /// <summary>
    /// 카드의 희귀도를 나타내는 열거형입니다.
    /// 카드의 등급과 강화 가능성을 결정하는 데 사용됩니다.
    /// </summary>
    public enum Rarity
    {
        /// <summary>
        /// 일반 등급 카드입니다.
        /// 가장 기본적인 등급으로, 흔하게 획득할 수 있습니다.
        /// </summary>
        Common,

        /// <summary>
        /// 고급 등급 카드입니다.
        /// 일반보다 조금 더 강력하고 희귀합니다.
        /// </summary>
        Uncommon,

        /// <summary>
        /// 희귀 등급 카드입니다.
        /// 강력한 효과를 가지고 있으며, 획득하기 어렵습니다.
        /// </summary>
        Rare,

        /// <summary>
        /// 영웅 등급 카드입니다.
        /// 매우 강력하고 독특한 효과를 가진 카드입니다.
        /// </summary>
        Epic,

        /// <summary>
        /// 전설 등급 카드입니다.
        /// 가장 강력하고 희귀한 카드로, 게임을 바꿀 수 있는 효과를 가집니다.
        /// </summary>
        Legendary
    }

    // ===== [기능 2] Property Enum =====
    /// <summary>
    /// 카드가 가질 수 있는 속성들을 정의하는 열거형입니다.
    /// 각 속성은 카드의 효과와 스탯에 영향을 미칩니다.
    /// </summary>
    public enum Property
    {
        Fire,
        Water,
        Light,
        Dark,
        Steel
    }
} 