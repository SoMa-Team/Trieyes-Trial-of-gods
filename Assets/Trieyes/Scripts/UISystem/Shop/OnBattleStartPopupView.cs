using System;
using System.Collections.Generic;
using CardSystem;
using CardViews;
using PrimeTween;
using Stats;
using UnityEngine;
using UnityEngine.UI;
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

            nextRoundButton.interactable = false;
            
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
            
            for (int i = logListRect.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(logListRect.transform.GetChild(i).gameObject);
            }
            
            statItems.Clear();
            cards.Clear();
            logItems.Clear();
            foreach (var cardTriggerParticle in particles)
            {
                if (cardTriggerParticle != null)
                    cardTriggerParticle.Deactivate();
            }
            particles.Clear();
        }
        
        [Header("====== 상단 카드 ======")]
        [SerializeField] private CardTriggerSlot cardSlotPrefab;
        [SerializeField] private CardView cardViewPrefab;

        [SerializeField] private RectTransform deckViewRect;
        [SerializeField] private RectTransform deckViewRectTarget;

        [Header("====== 사이드 스탯 ======")]
        [SerializeField] private RectTransform statListRect;
        [SerializeField] private StatItemInBattleStartPopupView statItemPrefab;

        [Header("====== 파티클 ======")]
        [SerializeField] private CardTriggerParticle particlePrefab;

        [Header("====== 하단 로그 ======")]
        [SerializeField] private RectTransform logListRect;
        [SerializeField] private StatChangeLogItem logItemPrefab;
        [SerializeField] private Button nextRoundButton; 

        private void InitCards()
        {
            var character = ShopSceneManager.Instance.mainCharacter;

            var total = character.deck.Cards.Count;
            if (total == 1)
            {
                var card = character.deck.Cards[0];
                AttachCard(card, 0.5f);
                return;
            }
            
            for (int i = 0; i < total; i++)
            {
                var card = character.deck.Cards[i];
                AttachCard(card, (float)i / (total - 1));
            }
        }

        private const int statItemHeight = 50 - 3;
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
            var character = ShopSceneManager.Instance.mainCharacter;

            int index = 0;
            foreach (StatType statType in applyStatLists)
            {
                var statValue = character.statSheet[statType].Value;
                var statItem = Instantiate(statItemPrefab, statListRect);
                statItem.rect.anchoredPosition = new Vector2(0, -statItemHeight * index++);
                statItem.Activate(statType, statValue, false);
                
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
            sizeDelta.x = targetSize.x / deckViewRect.lossyScale.x;
            deckViewRect.sizeDelta = sizeDelta;
        }

        private void AttachCard(Card card, float position)
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
        
        public void AnimateTriggerEvent(Queue<TriggerInfo> triggerQueue)
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
                nextRoundButton.interactable = true;
            });
        }
        
        private const float StartDuration = 0.5f;
        private const float DurationScaleMultiplier = 0.98f;
        private const float MinimumDuration = 0.05f;

        private float GetAnimationDuration(int triggerCount)
        {
            return Mathf.Max(StartDuration * Mathf.Pow(DurationScaleMultiplier, triggerCount), MinimumDuration);
        }

        private RectTransform rectLastTriggeredCard;
        private int TriggerCard(Sequence sequence, CardTriggerInfo cardTriggerInfo, int triggerCount)
        {
            var card = cardTriggerInfo.card;

            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].Item1 != card)
                    continue;

                var cardSlot = cards[i].Item2;
                sequence.Chain(cardSlot.TriggerCard(GetAnimationDuration(triggerCount)));
                rectLastTriggeredCard = cardSlot.rectTransform;
                return 1;
            }

            return 0;
        }

        private List<StatChangeLogItem> logItems = new List<StatChangeLogItem>();
        private int TriggerStat(Sequence sequence, StatTriggerInfo triggerStatTriggerInfo, int triggerCount)
        {
            var statType = triggerStatTriggerInfo.statType;
            var modifier = triggerStatTriggerInfo.modifier;
            var duration = GetAnimationDuration(triggerCount);
            
            if (!statItems.ContainsKey(statType))
                return 0;
            
            var statItem = statItems[statType];
            var rectCard = rectLastTriggeredCard;
            
            sequence.Group(Tween.Delay(0).OnComplete(() =>
            {
                statItem.TriggerModifier(modifier, true);
                
                var logItem = Instantiate(logItemPrefab, logListRect);
                logItem.Activate(statType, statItem.statValue, modifier, 0);
                logItems.Add(logItem);

                for (int i = 0; i < logItems.Count; i++)
                {
                    Tween.StopAll(logItems[i].rect);
                    logItems[i].AnimateToPosition(duration * 0.9f, logItems.Count - 1 - i);
                }

                var nextValue = modifier.getNextValue(statItem.statValue);
                var particleScale = Mathf.Clamp(statItem.statValue != 0 ? nextValue / statItem.statValue - 1 : 0, 0, 1);
                TriggerCardStatParticle(rectCard, statItem.rect, duration, particleScale);
                
                Tween.Delay(duration).OnComplete(() =>
                {
                    statItem.TriggerEnd();
                });
            }));
            
            return 1;
        }

        private List<CardTriggerParticle> particles = new();
        private void TriggerCardStatParticle(RectTransform rectCard, RectTransform rectStat, float duration, float scale)
        {
            var particle = Instantiate(particlePrefab, transform);
            particles.Add(particle);
            particle.Activate(rectCard, rectStat, duration, scale);
        }

        public void OnClickNextRound()
        {
            Deactivate();
            ShopSceneManager.Instance.StartNextBattleOnPopup();
        }
    }
}