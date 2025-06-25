using UnityEngine;
using System.Collections.Generic;
using Stats;
using CardActions;
using System;
using CharacterSystem;

namespace CardSystem
{
    using CardID = Int32;
    public class Card
    {
        private static int idCounter = 0;
        [Header("Card Info")]
        public CardID cardId;
        public CardAction cardActionSO;     // ScriptableObject 참조!
        public CardStat cardStats;
        public CardEnhancement cardEnhancement;
         Pawn owner;

        public Card(){
            this.cardId = idCounter++;
        }
        
        public void Activate(int level, int CardActionID, Pawn owner = null){
            if(CardActionFactory.Instance == null){
                Debug.LogWarning("CardActionFactory가 초기화되지 않았습니다.");
                return;
            }
            cardActionSO = CardActionFactory.Instance.Create(CardActionID);
            if(cardActionSO == null) {
                Debug.LogWarning($"CardAction 생성 실패! CardActionID={CardActionID}");
                return;
            }
            cardStats = new CardStat(cardActionSO.properties, level);
            cardEnhancement = new CardEnhancement(level, 0);
        }

        public void Deactivate(){
            
        }

        public void TriggerCardEvent(Utils.EventType eventType, object param = null)
        {
            cardActionSO?.OnEvent(owner, null, eventType, param);
        }
        
        public void SetOwner(CharacterSystem.Pawn pawn)
        {
            owner = pawn;
        }
    }
} 