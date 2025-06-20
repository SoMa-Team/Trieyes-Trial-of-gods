using UnityEngine;
using System.Collections.Generic;
using Stats;
using CardActions;
using Actors;

namespace CardSystem
{
    public class Card : MonoBehaviour
    {
        [Header("Card Info")]
        public int cardId;
        public string cardName;
        public Property[] properties;
        public Rarity cardRarity;

        [Header("Enhancement")]
        public CardEnhancement cardEnhancement;

        [Header("Stats & Action")]
        public CardStat[] cardStats;
        public CardAction cardAction;

        private Pawn owner;
        
        public void Initialize(Pawn cardOwner)
        {
            cardAction?.SetOwner(cardOwner);
        }

        public void TriggerCardEvent(Utils.EventType eventType, object param = null)
        {
            cardAction?.OnEvent(eventType, param);
        }
        
        public void SetOwner(Pawn pawn)
        {
            cardAction?.SetOwner(pawn);
        }
    }
} 