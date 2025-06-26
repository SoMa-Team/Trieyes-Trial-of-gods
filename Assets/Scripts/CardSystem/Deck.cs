using System.Collections.Generic;
using Utils;
using CharacterSystem;
using Stats;
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 카드 덱을 관리하는 클래스입니다.
    /// </summary>
    public class Deck : IEventHandler
    {
        [Header("Deck Setup")]
        private List<Card> cards = new();
        public IReadOnlyList<Card> Cards => cards;

        private Pawn owner;
        private bool isPersistent;
        private Dictionary<Utils.EventType, int> eventTypeCount = new();//이벤트 타입별 해당 이벤트를 갖고 있는 카드의 개수

        // ===== [기능 3] 카드 호출 순서 관리 =====
        private List<int> cardCallCounts;
        private List<int> cardCallOrder;
        private int maxIterations;

        public void Initialize(Pawn owner, bool isPersistent)
        {
            this.owner = owner;
            this.isPersistent = isPersistent;
            
            // 소유자 설정
            foreach (var card in cards)
            {
                card?.SetOwner(owner);
            }
            
            Clear();

            Debug.Log($"<color=green>[DECK] {owner?.gameObject.name} ({owner?.GetType().Name}) initialized with {cards.Count} cards (isPersistent: {isPersistent})</color>");
        }

        public void Clear()
        {
            if (!isPersistent)
            {
                Debug.Log($"<color=yellow>[DECK] {owner?.gameObject.name} clearing all cards (isPersistent: {isPersistent})</color>");
                cards.Clear();
            }
            else
            {
                Debug.Log($"<color=green>[DECK] {owner?.gameObject.name} preserving {cards.Count} cards (isPersistent: {isPersistent})</color>");
            }
            
            cardCallCounts = (cards.Count > 0) ? new List<int>(new int[cards.Count]) : new List<int>();
            cardCallOrder ??= new List<int>();
            cardCallOrder.Clear();
            maxIterations = cards.Count * 100;
        }

        // ===== [기능 4] 이벤트 처리 =====
        public void OnEvent(Utils.EventType eventType, object param)
        {
            Debug.Log($"<color=cyan>[DECK] {owner?.gameObject.name} ({owner?.GetType().Name}) received {eventType} event</color>");

            switch (eventType)
            {
                case Utils.EventType.OnBattleStart:
                    //CalcBaseStat();
                    CalcActionInitOrder();
                    CalcActionInitStat(Utils.EventType.OnBattleSceneChange);
                    break;
                case Utils.EventType.OnBattleEnd:
                    cardCallOrder ??= new List<int>();
                    cardCallOrder.Clear();
                    cardCallCounts = new List<int>(new int[cards.Count]);
                    owner?.statSheet.ClearBuffs();
                    break;
                case Utils.EventType.OnCardPurchase:
                    if (param is Card newCard) AddCard(newCard);
                    break;
                case Utils.EventType.OnCardRemove:
                    if (param is Card removedCard) RemoveCard(removedCard);
                    break;
                default:
                    // 이벤트 처리 최적화
                    if (eventTypeCount != null && !eventTypeCount.ContainsKey(eventType))
                    {
                        Debug.Log($"<color=grey>[DECK] {owner?.gameObject.name}: No card reacts to {eventType}, skipping.</color>");
                        return;
                    }
                    foreach (var card in cards)
                    {
                        // 이 카드가 해당 이벤트에 반응할 때만 호출!
                        if (card.cardActionSO != null && card.cardActionSO.eventTypes.Contains(eventType))
                        {
                            card.TriggerCardEvent(eventType, this, param);
                        }
                    }
                    break;
            }
        }

        // ===== [기능 1] 카드 리스트 및 생성 =====
        public void AddCard(Card card)
        {
            if (card != null)
            {
                cards.Add(card);
                card.SetOwner(owner);
                foreach (var evt in card.cardActionSO.eventTypes)
                {
                    if (eventTypeCount.ContainsKey(evt))
                        eventTypeCount[evt]++;
                    else
                        eventTypeCount[evt] = 1;
                }
            }
        }

        public void RemoveCard(Card card)
        {
            if (card != null && cards.Remove(card))
            {
                foreach (var evt in card.cardActionSO.eventTypes)
                {
                    if (eventTypeCount.TryGetValue(evt, out int count))
                    {
                        if (count <= 1)
                            eventTypeCount.Remove(evt);
                        else
                            eventTypeCount[evt] = count - 1;
                    }
                }
            }
        }

        // ===== [기능 2] 덱 스탯 및 카드 액션 초기화 =====
        public void CalcBaseStat()
        {
            if (owner != null)
            {
                owner.statSheet.ClearBuffs();
                foreach (Card card in cards){
                    foreach (StatType statType in System.Enum.GetValues(typeof(StatType))){
                        int value = card.cardStats.statSheet[statType].Value;
                        if (value == 0) continue; // 0은 굳이 버프로 추가하지 않아도 됨

                        var buff = new StatModifier(value, BuffOperationType.Additive);
                        owner.statSheet[statType].AddBuff(buff);
                    }
                }
            }
        }

        /// <summary>
        /// 카드 호출 순서 계산 (각 Card의 CardActionSO에 CalcActionInitOrder 이벤트 전파)
        /// </summary>
        public void CalcActionInitOrder()
        {
            if (cards.Count == 0) return;
            cardCallCounts = new List<int>(new int[cards.Count]);
            cardCallOrder ??= new List<int>();
            cardCallOrder.Clear();
            maxIterations = cards.Count * 100;

            int currentCardIndex = 0;
            int iterationCount = 0;

            while (currentCardIndex < cards.Count && iterationCount < maxIterations)
            {
                cardCallCounts[currentCardIndex]++;
                cardCallOrder.Add(currentCardIndex);

                // SO 기반 이벤트 전파
                cards[currentCardIndex]?.TriggerCardEvent(Utils.EventType.CalcActionInitOrder, this, currentCardIndex);

                currentCardIndex++;
                iterationCount++;
            }

            Debug.Log($"<color=white>[DECK] {owner?.gameObject.name} final call order: [{string.Join("->", cardCallOrder)}]</color>");
        }

        /// <summary>
        /// 계산된 순서대로 카드 액션의 이벤트를 발동
        /// </summary>
        public void CalcActionInitStat(Utils.EventType eventType, object param = null)
        {
            Debug.Log("CalcActionInitStat!!");
            if (cardCallOrder == null)
            {
                cardCallOrder = new List<int>();
                Debug.LogWarning($"<color=yellow>[DECK] {owner?.gameObject.name} cardCallOrder was null, initialized empty list</color>");
                return;
            }

            foreach (int cardIndex in cardCallOrder)
            {
                if (cardIndex < cards.Count)
                {
                    Debug.Log($"CalcActionInitStat: {cards[cardIndex].cardActionSO.cardName}");
                    cards[cardIndex]?.TriggerCardEvent(eventType, this, param);
                }
            }
        }

        // ===== [기능 5] 카드 호출 횟수 관리 =====
        public int GetCardCallCount(int cardIndex)
        {
            if (cardIndex >= 0 && cardIndex < cardCallCounts.Count)
                return cardCallCounts[cardIndex];
            return 0;
        }
        public void SetCardCallCount(int cardIndex, int count)
        {
            if (cardIndex >= 0 && cardIndex < cardCallCounts.Count)
                cardCallCounts[cardIndex] = count;
        }
        public void IncrementCardCallCount(int cardIndex, int increment = 1)
        {
            if (cardIndex >= 0 && cardIndex < cardCallCounts.Count)
                cardCallCounts[cardIndex] += increment;
        }
        public List<int> GetCallOrder() => cardCallOrder;
        public Pawn GetOwner() => owner;
    }
}
