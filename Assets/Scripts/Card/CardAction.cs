namespace Card
{
    public abstract class CardAction
    {
        public abstract void TryActivate(Card card, object context = null);
    }

    public class CardAction001 : CardAction
    {
        public override void TryActivate(Card card, object context = null)
        {
            // 예시: 특정 조건(if) 만족 시 스탯 증가, AttackComp 생성 등
            // if (조건) { ... }
        }
    }

    public class CardAction002 : CardAction
    {
        public override void TryActivate(Card card, object context = null)
        {
            // 예시: 다른 조건
        }
    }
} 