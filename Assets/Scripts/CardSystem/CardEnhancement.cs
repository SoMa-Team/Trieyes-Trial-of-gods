using Stats;

namespace CardSystem
{
    public class CardEnhancement
    {
        // ===== [기능 1] 카드 강화 정보 및 생성 =====
        public IntegerStatValue level; // 카드의 레벨
        public IntegerStatValue exp;   // 카드의 경험치

        public CardEnhancement(int initialLevel, int initialExp, int maxLevel = 99, int maxExp = 100)
        {
            level = new IntegerStatValue(initialLevel, maxLevel, 1);
            exp = new IntegerStatValue(initialExp, maxExp, 0);
        }
        
        // ===== [기능 2] 카드 합성 =====
        /// <summary>
        /// 현재 카드에 다른 카드의 경험치를 합성합니다.
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
        /// 경험치가 충분한지 확인하고 레벨업을 수행합니다.
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
    }
} 