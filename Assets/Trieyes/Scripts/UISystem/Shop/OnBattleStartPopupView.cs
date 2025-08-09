using System;
using System.Collections.Generic;
using System.IO;
using CardSystem;
using CardViews;
using PrimeTween;
using Stats;
using UnityEngine;
using Utils;

namespace UISystem
{
    public class OnBattleStartPopupView : MonoBehaviour
    {
        public static OnBattleStartPopupView Instance { get; private set; }

        private void Awake()
        {
            if (Instance is not null)
            {
                Destroy(Instance);
                return;
            }

            Instance = this;
        }

        public void Activate()
        {
            OnResize();
            
            InitCards();
            InitStat();
        }

        [SerializeField] private CardTriggerSlot cardSlotPrefab;
        [SerializeField] private CardView cardViewPrefab;

        [SerializeField] private RectTransform deckViewRect;
        [SerializeField] private RectTransform deckViewRectTarget;

        [SerializeField] private RectTransform statListRect;
        [SerializeField] private StatItemInBattleStartPopupView statItemPrefab;

        private void InitCards()
        {
            var character = NewShopSceneManager.Instance.mainCharacter;

            var total = character.deck.Cards.Count;
            for (int i = 0; i < total; i++)
            {
                var card = character.deck.Cards[i];
                AddCard(card, (float)i / (total - 1));
            }
        }

        private const int statItemHeight = 50;
        private Dictionary<StatType, StatItemInBattleStartPopupView> statItems = new();
        private static readonly StatType[] applyStatLists =
        {
            StatType.AttackPower,
            StatType.CriticalRate,
            StatType.CriticalDamage,
            StatType.AttackSpeed,
            StatType.SkillCooldownReduction,
            StatType.Reflect,
            StatType.Health,
            StatType.Defense,
            StatType.HealthRegen,
            StatType.LifeSteal,
            StatType.Evasion,
            StatType.MoveSpeed,
            StatType.ItemMagnet,
            StatType.GoldDropRate,
        };

        private void InitStat()
        {
            var character = NewShopSceneManager.Instance.mainCharacter;

            int index = 0;
            foreach (StatType statType in applyStatLists)
            {
                var statValue = character.statSheet[statType].Value;
                // if (statValue == 0)
                //     continue;
                
                var statItem = Instantiate(statItemPrefab, statListRect);
                statItem.rect.anchoredPosition = new Vector2(0, -statItemHeight * index++);
                statItem.Activate(statType, character.statSheet[statType].Value, false);
                
                statItems.Add(statType, statItem);
            }

            var sizeDelta = statListRect.sizeDelta;
            sizeDelta.y = statItemHeight * index;
            statListRect.sizeDelta = sizeDelta;
        }

        public void OnResize()
        {
            Canvas.ForceUpdateCanvases();
            
            // TODO : Resize가 화면 크기 변화에도 동작하도록 수정 필요!
            var size = Vector2.Scale(deckViewRect.rect.size, deckViewRect.lossyScale);
            var targetSize = Vector2.Scale(deckViewRectTarget.rect.size, deckViewRectTarget.lossyScale);

            var scale = targetSize.y / size.y;
            deckViewRect.localScale *= scale * Vector2.one;
            var sizeDelta = deckViewRect.sizeDelta;
            sizeDelta.x = targetSize.x / deckViewRect.localScale.x;
            deckViewRect.sizeDelta = sizeDelta;
        }

        private void AddCard(Card card, float position)
        {
            const int cardHeight = 590;

            var cardSlot = Instantiate(cardSlotPrefab, deckViewRect);
            var availableWidth = deckViewRect.rect.width - cardHeight;
            cardSlot.rectTransform.anchoredPosition = new Vector2(availableWidth * position, 0);
            cardSlot.transform.localScale = Vector3.one;

            var cardView = Instantiate(cardViewPrefab, cardSlot.gameObject.transform);
            cardView.rectTransform.anchoredPosition = Vector2.zero;
            cardView.transform.localScale = Vector3.one;

            cardView.SetCard(card);
            cards.Add((card, cardSlot));
        }
        
        private List<(Card, CardTriggerSlot)> cards = new List<(Card, CardTriggerSlot)>();
        
        public void AnimateTriggerEvent(Queue<TriggerInfo> triggerQueue, Action onComplete)
        {
            var sequence = Sequence.Create();
            int triggerCount = 0;
            foreach (var trigger in triggerQueue)
            {
                if (trigger.type == TriggerType.Card)
                    triggerCount += TriggerCard(sequence, trigger.cardTriggerInfo, triggerCount);
                else
                    triggerCount += TriggerStat(sequence, trigger.statTriggerInfo, triggerCount);
            }
            sequence.OnComplete(onComplete);
        }

        private float getAnimationDuration(int triggerCount)
        {
            return 0.5f * Mathf.Pow(0.99f, triggerCount);
        }

        private int TriggerCard(Sequence sequence, CardTriggerInfo cardTriggerInfo, int triggerCount)
        {
            var card = cardTriggerInfo.card;

            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].Item1 != card)
                    continue;

                var cardSlot = cards[i].Item2;
                var tween = cardSlot.TriggerCard(getAnimationDuration(triggerCount));
                sequence.Chain(tween);
                return 1;
            }

            return 0;
        }

        private int TriggerStat(Sequence sequence, StatTriggerInfo triggerStatTriggerInfo, int triggerCount)
        {
            var statType = triggerStatTriggerInfo.statType;
            var modifier = triggerStatTriggerInfo.modifier;
            var duration = getAnimationDuration(triggerCount);
            //
            // float pivotY = Single.NegativeInfinity;
            //
            // if (!statItems.ContainsKey(statType))
            // {
            //     var statItem = Instantiate(statItemPrefab, statListRect);
            //     statItem.Activate(statType, 0, true);
            //     statItem.rect.anchoredPosition = new Vector2(0, 0);
            //     statItem.CreateAlphaSequence(0, 0);
            //     
            //     sequence.Chain(statItem.CreateAlphaSequence(duration, 1));
            //     statItems.Add(statType, statItem);
            // }
            // else
            // {
            //     var statItem = statItems[statType];
            //     pivotY = statItem.rect.anchoredPosition.y;
            //     sequence.Chain(Tween.UIAnchoredPositionY(statItem.rect, 0, duration));
            // }
            //
            // var view = statItems[statType];
            // view.transform.SetAsLastSibling();
            // view.TriggerModifier(statType, modifier, true);
            //
            // foreach (var key in statItems.Keys)
            // {
            //     if (key == statType)
            //         continue;
            //     
            //     var statItem = statItems[key];
            //     if (statItem.rect.anchoredPosition.y > pivotY)
            //     {
            //         var targetY = statItem.rect.anchoredPosition.y + statItemHeight;
            //         sequence.Group(Tween.UIAnchoredPositionY(statItem.rect, targetY, duration)).OnComplete(() =>
            //         {
            //             statItem.TriggerEnd();
            //         });
            //     } 
            // }

            if (!statItems.ContainsKey(statType))
                return 0;

            var statItem = statItems[statType];
            sequence.Group(Tween.Custom(0f, 1f, duration, t =>
            {
                statItem.transform.rotation = Quaternion.Euler(0, 0, 360 * t);
            }).Group(Tween.Custom(0f, 1f, 0f, t =>
            {
                statItem.TriggerModifier(modifier, false);
            })));
            
            return 1;
        }
    }
}