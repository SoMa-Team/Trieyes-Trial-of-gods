using CardSystem;
using Stats;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0103_Stop: RelicAction
    {
        private const int addCardID = 2001; 
        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnRelicAdded:
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