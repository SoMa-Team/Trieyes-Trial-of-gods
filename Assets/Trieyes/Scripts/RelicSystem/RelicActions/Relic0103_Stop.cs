using CardSystem;
using Stats;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0103_Stop: RelicAction
    {   
        // Relic 103
        // 카드 슬롯 수가 1 증가합니다. [무효] 카드가 랜덤 위치에 장착됩니다.[무효] : 카드가 트리거 되면, 이 이후의 카드는 트리거 되지 않습니다.
        private const int addCardID = 2001; 
        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnRelicAdded:
                    if (param is not Relic relic)
                        return false;
                    if (relic != _relic)
                        return false;
                    
                    OnRelicAdded();
                    return true;
            }
            
            return base.OnEvent(eventType, param);
        }

        private void OnRelicAdded()
        {
            var owner = _relic.owner;
            owner.statSheet[StatType.DeckSize].AddToBasicValue(1);
            var card = CardFactory.Instance.CreateByID(addCardID);
            owner.deck.AddCard(card);
        }
    }
}