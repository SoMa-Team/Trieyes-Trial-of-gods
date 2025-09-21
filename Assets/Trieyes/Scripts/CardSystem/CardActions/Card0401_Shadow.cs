using UnityEngine;
using CharacterSystem;
using CardSystem;
using System.Collections.Generic;
using System;
using System.Linq;
using Utils;

namespace CardActions
{
    /// <summary>
    /// desc: 전투가 시작할 때 ,나 이외의 카드를 한 번 더 발동시킵니다.
    /// </summary>
    public class Card0401_Shadow : CardAction
    {
        public Card0401_Shadow()
        {
            
        }

        /// <summary>
        /// CalcActionInitOrder 이벤트에서 효과 발동.
        /// </summary>
        
        public override bool OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("[Shadow] owner 또는 deck이 정의되지 않았습니다.");
                return false;
            }

            if (eventType == Utils.EventType.CalcActionInitOrder)
            {
                if (param is ValueTuple<List<int>, int> tuple)
                {
                    int currentCardIndex = tuple.Item2;
                    int repeatCount = 1;
                    var cardCallOrder = tuple.Item1;
                    return HandleCalcActionInitOrder(deck, cardCallOrder, repeatCount, currentCardIndex);
                }
                return false;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                
            }

            return false;
        }

        /// <summary>
        /// 자기 자신을 제외한 모든 카드의 인덱스를 repeatCount번 callOrder에 추가.
        /// </summary>
        private bool HandleCalcActionInitOrder(Deck deck, List<int> cardCallOrder, int repeatCount, int currentCardIndex)
        {
            var deckSize = deck.Cards.Count;
            var isFirstTrigger = Enumerable.Range(0, currentCardIndex).All(i => cardCallOrder[i] != cardCallOrder[currentCardIndex]);

            if (!isFirstTrigger)
                return false;

            var cardIndex = cardCallOrder[currentCardIndex];
            List<int> insertCards = Enumerable.Range(0, repeatCount)
                .SelectMany(_ => Enumerable.Range(0, deckSize)
                    .Where(x => x != cardIndex))
                .ToList();
            
            cardCallOrder.InsertRange(currentCardIndex + 1, insertCards);
            return true;
        }
    }
}
