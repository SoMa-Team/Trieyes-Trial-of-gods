using System;
using System.Collections.Generic;
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
                if (param is ValueTuple<List<int>, int> tuple)
                {
                    var cardCallOrder = tuple.Item1;
                    var currentIndex = tuple.Item2;
                    cardCallOrder.RemoveRange(currentIndex + 1, cardCallOrder.Count - currentIndex - 1);
                    return true;
                }
            }
            
            return base.OnEvent(owner, deck, eventType, param);
        }
    }
}