using System;
using System.Collections.Generic;
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

        private int lastScreenWidth;
        private int lastScreenHeight;
        private void Update()
        {
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                OnResize();
                lastScreenWidth = Screen.width;
                lastScreenHeight = Screen.height;
            }
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            OnResize();
            InitCards();
            InitStat();
        }

        private void Deactivate()
        {
            gameObject.SetActive(false);

            for (int i = deckViewRect.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(deckViewRect.transform.GetChild(i).gameObject);
            }
            
            for (int i = statListRect.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(statListRect.transform.GetChild(i).gameObject);
            }
            
            statItems.Clear();
            cards.Clear();
        }

        [Header("====== 상단 카드 ======")]
        [SerializeField] private CardTriggerSlot cardSlotPrefab;
        [SerializeField] private CardView cardViewPrefab;

        [SerializeField] private RectTransform deckViewRect;
        [SerializeField] private RectTransform deckViewRectTarget;

        [Header("====== 사이드 스탯 ======")]
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
            const int cardWidth = 590;
            const int cardHeight = 860;

            var cardSlot = Instantiate(cardSlotPrefab, deckViewRect);
            var availableWidth = deckViewRect.rect.width - cardWidth;
            cardSlot.rectTransform.anchoredPosition = new Vector2(availableWidth * position + cardWidth * 0.5f, cardHeight * 0.5f);
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
            sequence.OnComplete(() =>
            {
                Deactivate();
                onComplete?.Invoke();
            });
        }

        private float GetAnimationDuration(int triggerCount)
        {
            return Mathf.Max(0.8f * Mathf.Pow(0.98f, triggerCount), 1f); // TODO : 시간 되돌리기
        }

        private int TriggerCard(Sequence sequence, CardTriggerInfo cardTriggerInfo, int triggerCount)
        {
            var card = cardTriggerInfo.card;

            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].Item1 != card)
                    continue;

                var cardSlot = cards[i].Item2;
                sequence.Chain(cardSlot.TriggerCard(GetAnimationDuration(triggerCount)));
                return 1;
            }

            return 0;
        }

        private int TriggerStat(Sequence sequence, StatTriggerInfo triggerStatTriggerInfo, int triggerCount)
        {
            var statType = triggerStatTriggerInfo.statType;
            var modifier = triggerStatTriggerInfo.modifier;
            var duration = GetAnimationDuration(triggerCount);
            
            if (!statItems.ContainsKey(statType))
                return 0;

            var statItem = statItems[statType];
            sequence.Group(Tween.Delay(0).OnComplete(() =>
            {
                statItem.TriggerModifier(modifier, true);
                Tween.Delay(duration).OnComplete(() =>
                {
                    statItem.TriggerEnd();
                });
            }));
            
            return 1;
        }
    }
}