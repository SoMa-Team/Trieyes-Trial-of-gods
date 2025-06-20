using System.Collections.Generic;
using Utils;
using Stats;
using UnityEngine;
using Actors;

namespace CardSystem
{
    /// <summary>
    /// 카드 덱을 관리하는 클래스입니다.
    /// 덱은 자체적으로 이벤트를 등록하고 처리할 수 있는 IEventHandler를 구현합니다.
    /// </summary>
    public class Deck : MonoBehaviour, IEventHandler
    {
        [Header("Deck Setup")]
        [SerializeField] private List<Card> cards = new();
        public IReadOnlyList<Card> Cards => cards;

        private Pawn owner;
        private bool isPersistent;
        
        // ===== [기능 3] 카드 호출 순서 관리 =====
        private List<int> cardCallCounts;
        private List<int> cardCallOrder = new();
        private int maxIterations;

        public void Initialize(Pawn owner, bool isPersistent)
        {
            this.owner = owner;
            this.isPersistent = isPersistent;
            
            foreach (var card in cards)
            {
                card?.Initialize(owner);
            }
            Clear();
        }

        public void Clear()
        {
            if (!isPersistent)
            {
                cards.Clear();
            }
            
            cardCallCounts = new List<int>(new int[cards.Count]);
            cardCallOrder.Clear();
            maxIterations = cards.Count * 100;
        }

        // ===== [기능 4] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            if (eventType == Utils.EventType.OnBattleStart)
            {
                // 1. 카드 호출 순서 계산
                CalcActionInitOrder();
                
                // 2. 덱의 기본 스탯 계산
                StatSheet deckStats = CalcBaseStat();
                
                // 3. Pawn에게 덱 스탯 더하고 전달
                if (owner != null)
                {
                    // 모든 스탯을 덱 스탯으로 덮어쓰기
                    foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
                    {
                        int deckStatValue = deckStats[statType].Value;
                        owner.statSheet[statType].SetBasicValue(deckStatValue);
                    }
                }
                
                // 4. 계산된 순서대로 카드 액션 초기화
                CalcActionInitStat(Utils.EventType.OnBattleSceneChange);
            }
            
            if (eventType == Utils.EventType.OnBattleEnd)
            {
                // 전투 종료 시 덱 상태 초기화
                cardCallOrder.Clear();
                cardCallCounts = new List<int>(new int[cards.Count]); // 0으로 초기화

                // 전투 종료 시 덱 스탯 초기화
                StatSheet deckStats = CalcBaseStat();
                if (owner != null)
                {
                    foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
                    {
                        int deckStatValue = deckStats[statType].Value;
                        owner.statSheet[statType].SetBasicValue(deckStatValue);
                    }
                }
            }
            
            if (eventType == Utils.EventType.OnCardPurchase)
            {
                // 카드 구매 시 덱에 추가
                if (param is Card newCard)
                {
                    AddCard(newCard);
                }
            }

            else
            {
                Debug.Log($"<color=red>[DECK] {owner?.gameObject.name} received event: {eventType}</color>");
                foreach (var card in cards)
                {
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
            cardCallOrder.Clear();
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

        /// <summary>
        /// 카드 호출 순서를 조정합니다.
        /// </summary>
        /// <param name="cardIndex">조정할 카드 인덱스</param>
        /// <param name="newIndex">새로운 위치</param>
        public void AdjustCardCallOrder(int cardIndex, int newIndex)
        {
            if (cardIndex >= 0 && cardIndex < cards.Count && newIndex >= 0 && newIndex < cards.Count)
            {
                // 호출 순서에서 해당 카드를 제거하고 새로운 위치에 삽입
                cardCallOrder.Remove(cardIndex);
                cardCallOrder.Insert(newIndex, cardIndex);
            }
        }

        /// <summary>
        /// 카드 호출 순서를 완전히 새로운 순서로 교체합니다.
        /// CardAction에서 호출 순서를 직접 수정할 때 사용합니다.
        /// </summary>
        /// <param name="newCallOrder">새로운 호출 순서</param>
        public void SetCallOrder(List<int> newCallOrder)
        {
            if (newCallOrder != null && newCallOrder.Count > 0)
            {
                cardCallOrder.Clear();
                cardCallOrder.AddRange(newCallOrder);
                
                // cardCallCounts도 새로운 순서에 맞게 조정
                cardCallCounts = new List<int>(new int[cards.Count]);
                
                Debug.Log($"<color=blue>[DECK] {owner?.gameObject.name} call order replaced: [{string.Join(", ", cardCallOrder)}]</color>");
            }
        }

        /// <summary>
        /// 특정 카드를 제외한 다른 모든 카드를 한 번 더 호출하도록 순서를 변경합니다.
        /// CardAction003과 같은 카드에서 사용합니다.
        /// </summary>
        /// <param name="excludeCardIndex">제외할 카드의 인덱스</param>
        public void AppendOtherCardsOnce(int excludeCardIndex)
        {
            if (cards.Count <= 1)
            {
                Debug.Log("<color=yellow>[DECK] Only one card in deck, no effect</color>");
                return;
            }

            if (excludeCardIndex < 0 || excludeCardIndex >= cards.Count)
            {
                Debug.LogWarning($"<color=red>[DECK] Invalid card index: {excludeCardIndex}</color>");
                return;
            }

            // 제외할 카드를 제외한 다른 모든 카드들을 한 번 더 추가
            List<int> cardsToAppend = new List<int>();
            
            for (int i = 0; i < cards.Count; i++)
            {
                if (i != excludeCardIndex) // 자기 자신 제외
                {
                    cardsToAppend.Add(i);
                }
            }

            // 기존 순서에 덧붙이기
            cardCallOrder.AddRange(cardsToAppend);
            
            Debug.Log($"<color=green>[DECK] {owner?.gameObject.name} appended other cards once (excluding {excludeCardIndex}): [{string.Join(", ", cardsToAppend)}]</color>");
        }
    }
} 