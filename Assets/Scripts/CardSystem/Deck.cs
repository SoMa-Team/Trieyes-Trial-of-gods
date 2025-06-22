using System.Collections.Generic;
using Utils;
using CharacterSystem;
using Stats;
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 카드 덱을 관리하는 클래스입니다.
    /// 덱은 자체적으로 이벤트를 등록하고 처리할 수 있는 IEventHandler를 구현합니다.
    /// </summary>
    public class Deck : MonoBehaviour, IEventHandler
    {
        [Header("Deck Setup")]
        [SerializeField] private List<Card> cards;
        public IReadOnlyList<Card> Cards => cards;

        private Pawn owner;
        private bool isPersistent;
        
        // ===== [기능 3] 카드 호출 순서 관리 =====
        private List<int> cardCallCounts;
        private List<int> cardCallOrder;
        private int maxIterations;

        public void Initialize(Pawn owner, bool isPersistent)
        {
            this.owner = owner;
            this.isPersistent = isPersistent;
            
            // 프리팹에서 설정한 카드들에 owner 참조 설정
            foreach (var card in cards)
            {
                card?.Initialize(owner);
            }
            
            // 전투 관련 상태만 초기화 (카드 리스트는 보존)
            Clear();
            
            Debug.Log($"<color=green>[DECK] {owner?.gameObject.name} ({owner?.GetType().Name}) initialized with {cards.Count} cards (isPersistent: {isPersistent})</color>");
        }

        public void Clear()
        {
            // isPersistent이 false인 경우에만 카드 리스트를 초기화
            if (!isPersistent)
            {
                Debug.Log($"<color=yellow>[DECK] {owner?.gameObject.name} clearing all cards (isPersistent: {isPersistent})</color>");
                cards.Clear();
            }
            else
            {
                Debug.Log($"<color=green>[DECK] {owner?.gameObject.name} preserving {cards.Count} cards (isPersistent: {isPersistent})</color>");
            }
            
            // 전투 관련 상태는 항상 초기화
            if (cards.Count > 0)
            {
                cardCallCounts = new List<int>(new int[cards.Count]);
            }
            else
            {
                cardCallCounts = new List<int>();
            }
            
            // cardCallOrder가 null이면 초기화
            if (cardCallOrder == null)
            {
                cardCallOrder = new List<int>();
            }
            else
            {
                cardCallOrder.Clear();
            }
            
            maxIterations = cards.Count * 100;
        }

        // ===== [기능 4] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            Debug.Log($"<color=cyan>[DECK] {owner?.gameObject.name} ({owner?.GetType().Name}) received {eventType} event</color>");
            
            if (eventType == Utils.EventType.OnBattleStart)
            {
                Debug.Log($"<color=cyan>[DECK] {owner?.gameObject.name} ({owner?.GetType().Name}) processing OnBattleStart</color>");
                // 1. 카드 호출 순서 계산
                CalcActionInitOrder();
                
                // 2. 계산된 순서대로 카드 액션 초기화 (기본 스탯은 보존)
                CalcActionInitStat(Utils.EventType.OnBattleSceneChange);
            }
            
            if (eventType == Utils.EventType.OnBattleEnd)
            {
                Debug.Log($"<color=cyan>[DECK] {owner?.gameObject.name} ({owner?.GetType().Name}) processing OnBattleEnd</color>");
                // 전투 종료 시 덱 상태 초기화
                if (cardCallOrder == null)
                {
                    cardCallOrder = new List<int>();
                }
                else
                {
                    cardCallOrder.Clear();
                }
                cardCallCounts = new List<int>(new int[cards.Count]); // 0으로 초기화

                // 전투 종료 시 모든 버프 제거 (기본 스탯은 보존)
                if (owner != null)
                {
                    foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
                    {
                        owner.statSheet[statType].ClearBuffs();
                    }
                }
            }
            
            if (eventType == Utils.EventType.OnCardPurchase)
            {
                Debug.Log($"<color=cyan>[DECK] {owner?.gameObject.name} ({owner?.GetType().Name}) processing OnCardPurchase</color>");
                // 카드 구매 시 덱에 추가
                if (param is Card newCard)
                {
                    AddCard(newCard);
                }
            }

            else
            {
                Debug.Log($"<color=cyan>[DECK] {owner?.gameObject.name} ({owner?.GetType().Name}) -> Processing {eventType} for {cards.Count} cards</color>");
                foreach (var card in cards)
                {
                    Debug.Log($"<color=cyan>[DECK] {owner?.gameObject.name} ({owner?.GetType().Name}) -> Card {card.cardAction.GetType().Name} processing {eventType}</color>");
                    card.cardAction.OnEvent(eventType, param);
                }
            }
        }

        // ===== [기능 1] 카드 리스트 및 생성 =====
        public void AddCard(Card card)
        {
            if (card != null)
            {
                cards.Add(card);
                card.Initialize(owner);
            }
        }

        public void RemoveCard(Card card)
        {
            if (card != null)
            {
                cards.Remove(card);
            }
        }

        // ===== [기능 2] 덱 스탯 및 카드 액션 초기화 =====
        /// <summary>
        /// 덱의 모든 카드 스탯을 합산하여 반환합니다.
        /// 이 합산된 스탯은 Pawn의 기본 스탯(statinfos)에 적용됩니다.
        /// </summary>
        /// <returns>합산된 StatInfo 리스트</returns>
        public StatSheet CalcBaseStat()
        {
            // 현재 Pawn의 기본 스탯을 반환 (카드 버프는 별도로 적용)
            if (owner != null)
            {
                StatSheet currentStats = new StatSheet();
                foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
                {
                    // 기본값만 가져오기 (버프 제외)
                    currentStats[statType].SetBasicValue(owner.statSheet[statType].Value);
                }
                return currentStats;
            }
            
            StatSheet emptyStat = new();
            return emptyStat;
        }

        /// <summary>
        /// 카드 호출 순서를 계산합니다.
        /// 각 카드의 CardAction에서 CalcActionInitOrder 이벤트를 받아 호출 순서를 결정합니다.
        /// </summary>
        public void CalcActionInitOrder()
        {
            if (cards.Count == 0) return;
            
            // 0으로 초기화된 리스트 생성
            cardCallCounts = new List<int>(new int[cards.Count]);
            
            // cardCallOrder가 null이면 초기화
            if (cardCallOrder == null)
            {
                cardCallOrder = new List<int>();
            }
            else
            {
                cardCallOrder.Clear();
            }
            
            maxIterations = cards.Count * 100;
            
            // 모든 카드에 CalcActionInitOrder 이벤트 전송
            int currentCardIndex = 0;
            int iterationCount = 0;
            
            while (currentCardIndex < cards.Count && iterationCount < maxIterations)
            {
                cardCallCounts[currentCardIndex]++;
                cardCallOrder.Add(currentCardIndex);
                
                if (cards[currentCardIndex]?.cardAction != null)
                {
                    cards[currentCardIndex].cardAction.OnEvent(Utils.EventType.CalcActionInitOrder, currentCardIndex);
                }
                currentCardIndex++;
                iterationCount++;
            }
            
            Debug.Log($"<color=white>[DECK] {owner?.gameObject.name} final call order: [{string.Join("->", cardCallOrder)}]</color>");
        }

        /// <summary>
        /// 계산된 순서대로 카드 액션의 이벤트를 발동합니다.
        /// </summary>
        /// <param name="eventType">발동할 이벤트 타입</param>
        /// <param name="param">이벤트 매개변수</param>
        public void CalcActionInitStat(Utils.EventType eventType, object param = null)
        {
            Debug.Log($"<color=lightblue>--- Calculating Stats for Event: {eventType} ---</color>");
            
            // cardCallOrder가 null이면 초기화
            if (cardCallOrder == null)
            {
                cardCallOrder = new List<int>();
                Debug.LogWarning($"<color=yellow>[DECK] {owner?.gameObject.name} cardCallOrder was null, initialized empty list</color>");
                return;
            }
            
            foreach (int cardIndex in cardCallOrder)
            {
                if (cardIndex < cards.Count && cards[cardIndex]?.cardAction != null)
                {
                    cards[cardIndex].cardAction.OnEvent(eventType, param);
                    if (owner != null)
                    {
                        Debug.Log($"<color=white>After Card[{cardIndex}] ({cards[cardIndex].cardName}): " +
                                  $"ATK={owner.statSheet[StatType.AttackPower].Value}, " +
                                  $"DEF={owner.statSheet[StatType.Defense].Value}</color>");
                    }
                }
            }
            Debug.Log($"<color=lightblue>--- Stat Calculation Finished ---</color>");
        }

        // ===== [기능 5] 카드 호출 횟수 관리 =====
        
        /// <summary>
        /// 특정 카드의 호출 횟수를 반환합니다.
        /// </summary>
        /// <param name="cardIndex">카드 인덱스</param>
        /// <returns>호출 횟수</returns>
        public int GetCardCallCount(int cardIndex)
        {
            if (cardIndex >= 0 && cardIndex < cardCallCounts.Count)
            {
                return cardCallCounts[cardIndex];
            }
            return 0;
        }

        /// <summary>
        /// 특정 카드의 호출 횟수를 설정합니다.
        /// </summary>
        /// <param name="cardIndex">카드 인덱스</param>
        /// <param name="count">설정할 호출 횟수</param>
        public void SetCardCallCount(int cardIndex, int count)
        {
            if (cardIndex >= 0 && cardIndex < cardCallCounts.Count)
            {
                cardCallCounts[cardIndex] = count;
            }
        }

        /// <summary>
        /// 특정 카드의 호출 횟수를 증가시킵니다.
        /// </summary>
        /// <param name="cardIndex">카드 인덱스</param>
        /// <param name="increment">증가시킬 값</param>
        public void IncrementCardCallCount(int cardIndex, int increment = 1)
        {
            if (cardIndex >= 0 && cardIndex < cardCallCounts.Count)
            {
                cardCallCounts[cardIndex] += increment;
            }
        }
  
        public List<int> GetCallOrder()
        {
            return cardCallOrder;
        }

        public Pawn GetOwner()
        {
            return owner;
        }
    }
} 