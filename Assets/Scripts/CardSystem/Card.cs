using UnityEngine;
using System.Collections.Generic;
using Stats;
using CardActions;

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

        private CharacterSystem.Pawn owner;
        
        public void Initialize(CharacterSystem.Pawn cardOwner)
        {
            owner = cardOwner;
            cardAction?.SetOwner(owner);
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