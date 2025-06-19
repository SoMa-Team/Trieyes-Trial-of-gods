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
            
        }
        
        // ===== [기능 2] 카드 합성 =====
        /// <summary>
        /// 현재 카드에 다른 카드의 경험치를 합성합니다.
        /// </summary>
        /// <param name="otherCardEnhancement">합성할 다른 카드의 CardEnhancement 객체</param>
    }
} 