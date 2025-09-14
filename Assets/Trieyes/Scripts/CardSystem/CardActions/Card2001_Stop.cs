using System;
using CardSystem;
using CharacterSystem;
using Utils;

namespace CardActions
{
    public class Card2001_Stop: CardAction
    {
        public override bool OnEvent(Pawn owner, Deck deck, EventType eventType, object param)
        {
            if (eventType == EventType.CalcActionInitOrder)
            {
                // TODO: 카드 정지 시키는 기믹

                return true;
            }
            
            return base.OnEvent(owner, deck, eventType, param);
        }
    }
}