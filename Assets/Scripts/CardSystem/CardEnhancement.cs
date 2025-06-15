using Utils;

namespace CardSystem
{
    public class CardEnhancement
    {
        public IntegerStatValue level; // 카드의 레벨
        public IntegerStatValue exp;   // 카드의 경험치

        public CardEnhancement(int initialLevel, int initialExp, int maxLevel = 99, int maxExp = 100)
        {
            level = new IntegerStatValue(initialLevel, maxLevel);
            exp = new IntegerStatValue(initialExp, maxExp);
        }
        
        /// <summary>
        /// 현재 카드에 다른 카드의 경험치를 합성합니다.
        /// </summary>
        /// <param name="otherCardEnhancement">합성할 다른 카드의 CardEnhancement 객체</param>
        public void Composite(CardEnhancement otherCardEnhancement)
        {
            // 경험치 추가
            this.exp.Add(val => val + otherCardEnhancement.exp.value);

            // 경험치가 MaxExp를 초과하면 레벨업 처리
            while (this.exp.value >= this.exp.maxValue.Value && this.level.value < this.level.maxValue.Value)
            {
                this.exp.value -= this.exp.maxValue.Value;
                this.level.Add(val => val + 1);
                // TODO: 레벨업에 따른 MaxExp 증가 로직이 있다면 여기에 추가
            }
            
            // 합성 후 경험치나 레벨이 Max 값을 초과하지 않도록 보장
            this.exp.value = System.Math.Min(this.exp.value, this.exp.maxValue.Value);
            this.level.value = System.Math.Min(this.level.value, this.level.maxValue.Value);
        }
    }
} 