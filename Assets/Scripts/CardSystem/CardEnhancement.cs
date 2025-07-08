using Stats;

namespace CardSystem
{
    /// <summary>
    /// 카드의 강화 시스템을 관리하는 클래스입니다.
    /// 카드의 레벨과 경험치를 관리하며, 카드 합성과 레벨업 기능을 제공합니다.
    /// </summary>
    public class CardEnhancement
    {
        // --- 필드 ---

        // ===== [기능 1] 카드 강화 정보 및 생성 =====
        /// <summary>
        /// 카드의 현재 레벨을 관리하는 IntegerStatValue 객체입니다.
        /// 최소값 1, 최대값 99로 설정됩니다.
        /// </summary>
        public IntegerStatValue level;

        /// <summary>
        /// 카드의 현재 경험치를 관리하는 IntegerStatValue 객체입니다.
        /// 최소값 0, 최대값 100으로 설정됩니다.
        /// </summary>
        public IntegerStatValue exp;

        // --- 생성자 ---

        /// <summary>
        /// CardEnhancement의 새 인스턴스를 초기화합니다.
        /// 레벨과 경험치의 초기값과 최대값을 설정합니다.
        /// </summary>
        /// <param name="initialLevel">초기 레벨</param>
        /// <param name="initialExp">초기 경험치</param>
        /// <param name="maxLevel">최대 레벨 (기본값: 99)</param>
        /// <param name="maxExp">최대 경험치 (기본값: 100)</param>
        public CardEnhancement(int initialLevel, int initialExp, int maxLevel = 99, int maxExp = 100)
        {
            level = new IntegerStatValue(initialLevel, maxLevel, 1);
            exp = new IntegerStatValue(initialExp, maxExp, 0);
        }

        // --- public 메서드 ---

        // ===== [기능 2] 카드 합성 =====
        /// <summary>
        /// 현재 카드에 다른 카드의 경험치를 합성합니다.
        /// 다른 카드의 경험치를 현재 카드에 추가하고 레벨업을 체크합니다.
        /// </summary>
        /// <param name="otherCardEnhancement">합성할 다른 카드의 CardEnhancement 객체</param>
        public void MergeCard(CardEnhancement otherCardEnhancement)
        {
            if (otherCardEnhancement == null) return;

            // 경험치 합성
            int totalExp = exp.Value + otherCardEnhancement.exp.Value;
            exp.SetBasicValue(totalExp);

            // 레벨업 체크
            CheckLevelUp();
        }

        /// <summary>
        /// 경험치를 추가하고 레벨업을 체크합니다.
        /// </summary>
        /// <param name="expAmount">추가할 경험치</param>
        public void AddExp(int expAmount)
        {
            exp.AddToBasicValue(expAmount);
            CheckLevelUp();
        }

        /// <summary>
        /// 카드의 총 경험치를 반환합니다.
        /// 총 경험치 = 레벨 * 7 + 현재 경험치
        /// </summary>
        /// <returns>카드의 총 경험치</returns>
        public int GetTotalExp()
        {
            return level.Value * 10 + exp.Value;
        }

        // --- private 메서드 ---

        /// <summary>
        /// 경험치가 충분한지 확인하고 레벨업을 수행합니다.
        /// 현재 레벨 * 10의 경험치가 필요하며, 레벨업 시 필요한 경험치만큼 차감됩니다.
        /// </summary>
        private void CheckLevelUp()
        {
            int currentExp = exp.Value;
            int requiredExp = level.Value * 10; // 레벨당 10 경험치 필요

            if (currentExp >= requiredExp)
            {
                level.AddToBasicValue(1);
                exp.AddToBasicValue(-requiredExp);
                CheckLevelUp(); // 재귀적으로 다음 레벨업 체크
            }
        }
        
        public CardEnhancement DeepCopy()
        {
            var clone = new CardEnhancement(
                this.level.Value,
                this.exp.Value
            );
            clone.level = this.level.DeepCopy();
            clone.exp = this.exp.DeepCopy();
            return clone;
        }
    }
} 