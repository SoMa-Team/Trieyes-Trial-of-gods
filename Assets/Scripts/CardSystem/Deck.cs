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
    public class Deck : IEventHandler
    {
        // ===== [기능 1] 덱 기본 정보 =====
        public bool isPersistent { get; private set; }  // 덱이 영구적인지 여부 (메인 캐릭터의 덱은 true, 적의 덱은 false)
        public Pawn owner { get; private set; }        // 덱의 소유자

        // ===== [기능 2] 덱 상태 관리 =====
        public List<Card> cards = new();               // 현재 덱의 카드들

        // ===== [기능 3] 카드 호출 순서 관리 =====
        private int[] cardCallCounts;                  // 각 카드의 호출 횟수 배열
        private List<int> cardCallOrder = new();       // 최종 카드 호출 순서
        private int maxIterations;                     // 최대 반복 횟수 (카드 개수 * 100)

        // ===== [기능 4] 덱 초기화/정리 =====
        public void Initialize(Pawn owner, bool isPersistent)
        {
            this.owner = owner;
            this.isPersistent = isPersistent;
            Clear();
            
            // 모든 카드의 CardAction에 owner 설정
            foreach (var card in cards)
            {
                card?.SetOwner(owner);
            }
        }

        public void Clear()
        {
            if (!isPersistent)
            {
                cards.Clear();
            }
            
            // 카드 호출 순서 관련 초기화
            cardCallCounts = new int[cards.Count];
            cardCallOrder.Clear();
            maxIterations = cards.Count * 100;
        }

        // ===== [기능 4] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            switch (eventType)
            {
                case Utils.EventType.OnBattleStart:
                    HandleDeckWhenBattleStart();
                    break;
                case Utils.EventType.OnBattleEnd:
                    HandleDeckWhenBattleEnd();
                    break;
                case Utils.EventType.OnCardPurchase:
                    HandleCardPurchase(param);
                    break;
                default:
                    break;
            }
        }

        private void HandleDeckWhenBattleStart()
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

        private void HandleDeckWhenBattleEnd()
        {
            if (!isPersistent)
            {
                // 적의 덱은 전투 종료 시 정리
                Clear();
                // 스탯 초기화
                if (owner != null)
                {
                    foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
                    {
                        owner.statSheet[statType].SetBasicValue(0);
                    }
                }
            }
            else
            {
                // 메인 캐릭터의 덱은 유지
            }
        }

        private void HandleCardPurchase(object param)
        {
            if (param is Card purchasedCard && isPersistent)
            {
                // 상점에서 카드 구매 시 덱에 추가
                cards.Add(purchasedCard);
            }
        }

        // ===== [기능 1] 카드 리스트 및 생성 =====
        public void AddCard(Card card)
        {
            if (card != null)
            {
                cards.Add(card);
                card.SetOwner(owner);
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
            
            // 초기화
            cardCallCounts = new int[cards.Count];
            cardCallOrder.Clear();
            maxIterations = cards.Count * 100;
            
            int currentCardIndex = 0;
            int iterationCount = 0;
            
            // 카드 순회 시작
            while (currentCardIndex < cards.Count && iterationCount < maxIterations)
            {
                // 현재 카드의 호출 횟수 증가
                cardCallCounts[currentCardIndex]++;
                
                // 카드 호출 순서에 추가
                cardCallOrder.Add(currentCardIndex);
                
                // 현재 카드의 CardAction에 CalcActionInitOrder 이벤트 전달
                if (cards[currentCardIndex]?.cardAction != null)
                {
                    cards[currentCardIndex].cardAction.OnEvent(Utils.EventType.CalcActionInitOrder, currentCardIndex);
                }
                
                // 다음 카드로 이동
                currentCardIndex++;
                iterationCount++;
            }
            
            Debug.Log($"<color=blue>[DECK] {owner?.gameObject.name} deck calculated call order: [{string.Join(", ", cardCallOrder)}]</color>");
        }

        /// <summary>
        /// 계산된 순서대로 카드 액션의 이벤트를 발동합니다.
        /// </summary>
        /// <param name="eventType">발동할 이벤트 타입</param>
        /// <param name="param">이벤트 매개변수</param>
        public void CalcActionInitStat(Utils.EventType eventType, object param = null)
        {
            // 계산된 순서대로 카드 액션 실행
            foreach (int cardIndex in cardCallOrder)
            {
                if (cardIndex >= 0 && cardIndex < cards.Count && cards[cardIndex]?.cardAction != null)
                {
                    cards[cardIndex].TriggerCardEvent(eventType, param);
                }
            }
        }

        // ===== [기능 5] 카드 호출 횟수 관리 =====
        
        /// <summary>
        /// 특정 카드의 호출 횟수를 반환합니다.
        /// </summary>
        /// <param name="cardIndex">카드 인덱스</param>
        /// <returns>호출 횟수</returns>
        public int GetCardCallCount(int cardIndex)
        {
            if (cardIndex >= 0 && cardIndex < cardCallCounts.Length)
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
            if (cardIndex >= 0 && cardIndex < cardCallCounts.Length)
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
            if (cardIndex >= 0 && cardIndex < cardCallCounts.Length)
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
    }
} 