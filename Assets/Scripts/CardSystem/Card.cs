using UnityEngine;
using System.Collections.Generic;
using Stats;
using CardActions;
using System;

namespace CardSystem
{
    using CardID = Int32;
    public class Card
    {
        private static int idCounter = 0;
        [Header("Card Info")]
        public CardID cardId;

        [Header("Enhancement")]
        public CardEnhancement cardEnhancement;

        [Header("Stats & Action")]
        public CardStat cardStats;
        public CardAction cardAction;

        private CharacterSystem.Pawn owner;

        public Card(){
            this.cardId = idCounter++;
        }
        
        public void Activate(int level, int CardActionID){
            if(CardActionFactory.Instance == null){
                Debug.LogWarning("CardActionFactory가 초기화되지 않았습니다.");
                return;
            }
            cardAction = CardActionFactory.Instance.Create(CardActionID);
            if(cardAction == null) {
                Debug.LogWarning($"CardAction 생성 실패! CardActionID={CardActionID}");
                return;
            }
            cardStats = new CardStat(cardAction.properties, level);
            cardEnhancement = new CardEnhancement(level, 0);
        }

        public void Deactivate(){
            CardActionFactory.Instance.Deactivate(cardAction);
        }

        public void TriggerCardEvent(Utils.EventType eventType, object param = null)
        {
            cardAction?.OnEvent(eventType, param);
        }
        
        public void SetOwner(CharacterSystem.Pawn pawn)
        {
            owner = pawn;
            cardAction?.SetOwner(pawn);
        }
    }
} 