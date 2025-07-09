using System.Collections.Generic;
using Utils;
using CharacterSystem;
using Stats;
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 카드 덱을 관리하는 클래스입니다.
    /// 카드의 추가/제거, 이벤트 처리, 카드 호출 순서 관리 등의 기능을 제공합니다.
    /// IEventHandler 인터페이스를 구현하여 게임 이벤트에 반응합니다.
    /// </summary>
    public class Deck : IEventHandler
    {
        [Header("Deck Setup")]
        /// <summary>
        /// 덱에 포함된 카드들의 리스트입니다.
        /// </summary>
        private List<Card> cards = new();

        /// <summary>
        /// 덱의 카드 리스트에 대한 읽기 전용 접근자입니다.
        /// </summary>
        public IReadOnlyList<Card> Cards => cards;

        /// <summary>
        /// 덱을 소유한 캐릭터(Pawn)입니다.
        /// </summary>
        private Pawn owner;

        /// <summary>
        /// 덱이 영구적인지 여부를 나타냅니다.
        /// true인 경우 전투 종료 후에도 카드가 유지됩니다.
        /// </summary>
        private bool isPersistent;

        /// <summary>
        /// 이벤트 타입별로 해당 이벤트를 갖고 있는 카드들의 개수를 관리하는 딕셔너리입니다.
        /// 이벤트 처리 최적화를 위해 사용됩니다.
        /// </summary>
        private Dictionary<Utils.EventType, int> eventTypeCount = new();
        public IReadOnlyDictionary<Utils.EventType, int> EventTypeCount => eventTypeCount;
        
        // ===== [기능 3] 카드 호출 순서 관리 =====
        /// <summary>
        /// 각 카드의 호출 횟수를 추적하는 리스트입니다.
        /// </summary>
        private List<int> cardCallCounts;

        /// <summary>
        /// 카드 호출 순서를 저장하는 리스트입니다.
        /// </summary>
        private List<int> cardCallOrder;

        /// <summary>
        /// 카드 호출 순서 계산 시 무한 루프를 방지하기 위한 최대 반복 횟수입니다.
        /// </summary>
        private int maxIterations;

        public void Activate(Pawn owner, bool isPersistent)
        {
            this.owner = owner;
            this.isPersistent = isPersistent;
            
            // 소유자 설정
            foreach (var card in cards)
            {
                card?.SetOwner(owner);
            }
            
            Clear();

            eventTypeCount[Utils.EventType.OnBattleSceneChange] = 1;
            eventTypeCount[Utils.EventType.OnBattleEnd] = 1;
            eventTypeCount[Utils.EventType.OnCardPurchase] = 1;
            eventTypeCount[Utils.EventType.OnCardRemove] = 1;

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
        /// <summary>
        /// IEventHandler 인터페이스 구현 메서드입니다.
        /// 게임 이벤트에 반응하여 적절한 처리를 수행합니다.
        /// </summary>
        /// <param name="eventType">발생한 이벤트 타입</param>
        /// <param name="param">이벤트와 함께 전달된 매개변수</param>
        public void OnEvent(Utils.EventType eventType, object param)
        {
            Debug.Log($"<color=cyan>[DECK] {owner?.gameObject.name} ({owner?.GetType().Name}) received {eventType} event</color>");

            switch (eventType)
            {
                case Utils.EventType.OnBattleSceneChange:
                    CalcBaseStat();
                    CalcActionInitOrder();
                    CalcActionInitStat(Utils.EventType.OnBattleSceneChange);
                    break;
                case Utils.EventType.OnBattleEnd:
                    cardCallOrder.Clear();
                    cardCallCounts = new List<int>(new int[cards.Count]);
                    EventProcessor(eventType, param);
                    owner?.statSheet.ClearBuffs();
                    break;
                case Utils.EventType.OnCardPurchase:
                    if (param is Card newCard) AddCard(newCard);
                    break;
                case Utils.EventType.OnCardRemove:
                    if (param is Card removedCard) RemoveCard(removedCard);
                    break;
                default:
                    EventProcessor(eventType, param);
                    break;
            }
        }

        public void EventProcessor(Utils.EventType eventType, object param)
        {
            foreach (var card in cards)
            {
                // 이 카드가 해당 이벤트에 반응할 때만 호출!
                if (card.cardAction != null && card.eventTypes.Contains(eventType))
                {
                    card.TriggerCardEvent(eventType, this, param);
                }
            }
        }

        // ===== [기능 1] 카드 리스트 및 생성 =====
        /// <summary>
        /// 덱에 새로운 카드를 추가합니다.
        /// 카드의 소유자를 설정하고 이벤트 타입 카운터를 업데이트합니다.
        /// </summary>
        /// <param name="card">추가할 카드</param>
        public void AddCard(Card card)
        {
            if (card != null)
            {
                cards.Add(card);
                card.SetOwner(owner);
                foreach (var evt in card.eventTypes)
                {
                    if (eventTypeCount.ContainsKey(evt))
                        eventTypeCount[evt]++;
                    else
                        eventTypeCount[evt] = 1;
                }
            }
        }

        /// <summary>
        /// 덱에서 카드를 제거합니다.
        /// 이벤트 타입 카운터도 함께 업데이트합니다.
        /// </summary>
        /// <param name="card">제거할 카드</param>
        public void RemoveCard(Card card)
        {
            if (card != null && cards.Remove(card))
            {
                foreach (var evt in card.eventTypes)
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
        
        public void SwapCards(Card cardA, Card cardB)
        {
            int idxA = cards.IndexOf(cardA);
            int idxB = cards.IndexOf(cardB);

            Debug.Log($"SwapCards: cardA({cardA?.cardName ?? "null"}) idxA: {idxA}, cardB({cardB?.cardName ?? "null"}) idxB: {idxB}");
            
            if (idxA < 0 || idxB < 0)
            {
                Debug.LogError($"card not found in deck");
                return;
            }

            // 카드 스왑
            (cards[idxA], cards[idxB]) = (cards[idxB], cards[idxA]);
        }

        /// <summary>
        /// 같은 이름의 두 카드를 합치는 메서드입니다.
        /// GetTotalExp가 높은 카드에 낮은 카드의 총 경험치를 합치고, 낮은 카드는 덱에서 제거합니다.
        /// </summary>
        /// <param name="cardA">합칠 첫 번째 카드</param>
        /// <param name="cardB">합칠 두 번째 카드</param>
        /// <returns>합치기가 성공했는지 여부</returns>
        public bool MergeCards(Card cardA, Card cardB)
        {
            Debug.Log("Merge Cards 호출");
            // 카드가 덱에 있는지 확인
            if (!cards.Contains(cardA) || !cards.Contains(cardB))
            {
                Debug.LogError("MergeCards: 하나 이상의 카드가 덱에 없습니다.");
                return false;
            }

            // 같은 이름의 카드인지 확인
            if (cardA.cardName != cardB.cardName)
            {
                Debug.LogError("MergeCards: 다른 이름의 카드는 합칠 수 없습니다.");
                return false;
            }

            int totalExpA = cardA.cardEnhancement.GetTotalExp();
            int totalExpB = cardB.cardEnhancement.GetTotalExp();

            Card higherExpCard, lowerExpCard;

            // GetTotalExp가 높은 카드와 낮은 카드 구분
            if (totalExpA >= totalExpB)
            {
                higherExpCard = cardA;
                lowerExpCard = cardB;
            }
            else
            {
                higherExpCard = cardB;
                lowerExpCard = cardA;
            }

            // 낮은 카드의 총 경험치를 높은 카드에 추가
            Debug.Log($"MergeCards: {higherExpCard.cardName}의 총 경험치: {higherExpCard.cardEnhancement.GetTotalExp()}");
            Debug.Log($"MergeCards: {lowerExpCard.cardName}의 총 경험치: {lowerExpCard.cardEnhancement.GetTotalExp()}");
            higherExpCard.cardEnhancement.AddExp(lowerExpCard.cardEnhancement.GetTotalExp());
            Debug.Log($"MergeCards: {higherExpCard.cardName}의 총 경험치: {higherExpCard.cardEnhancement.GetTotalExp()}");

            higherExpCard.RefreshStats();

            // 낮은 카드를 덱에서 제거
            RemoveCard(lowerExpCard);

            Debug.Log($"카드 합치기 완료: {higherExpCard.cardName} (총 경험치: {higherExpCard.cardEnhancement.GetTotalExp()})");
            return true;
        }

        // ===== [기능 2] 덱 스탯 및 카드 액션 초기화 =====
        /// <summary>
        /// 덱의 기본 스탯을 계산합니다.
        /// 모든 카드의 스탯을 소유자에게 버프로 적용합니다.
        /// </summary>
        public void CalcBaseStat()
        {
            if (owner != null)
            {
                owner.statSheet.ClearBuffs();
                foreach (Card card in cards){
                    foreach (var statPair in card.cardStats.stats)
                    {
                        int value = statPair.value.Value;
                        if (value == 0) continue; // 0이면 버프 필요 없음

                        var buff = new StatModifier(value, BuffOperationType.Additive);
                        owner.statSheet[statPair.type].AddBuff(buff);
                    }
                }
            }
        }

        /// <summary>
        /// 카드 호출 순서를 계산합니다.
        /// 각 카드의 CardActionSO에 CalcActionInitOrder 이벤트를 전파하여 순서를 결정합니다.
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

                var card = cards[currentCardIndex];

                if (card != null)
                {
                    card.TriggerCardEvent(Utils.EventType.CalcActionInitOrder, this, (card, currentCardIndex));
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
        /// <param name="param">이벤트와 함께 전달할 매개변수 (선택사항)</param>
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
                    Debug.Log($"CalcActionInitStat: {cards[cardIndex].cardName}");
                    cards[cardIndex]?.TriggerCardEvent(eventType, this, cards[cardIndex]);
                }
            }
        }

        // ===== [기능 5] 카드 호출 횟수 관리 =====
        /// <summary>
        /// 특정 카드의 호출 횟수를 반환합니다.
        /// </summary>
        /// <param name="cardIndex">카드의 인덱스</param>
        /// <returns>호출 횟수</returns>
        public int GetCardCallCount(int cardIndex)
        {
            if (cardIndex >= 0 && cardIndex < cardCallCounts.Count)
                return cardCallCounts[cardIndex];
            return 0;
        }

        /// <summary>
        /// 특정 카드의 호출 횟수를 설정합니다.
        /// </summary>
        /// <param name="cardIndex">카드의 인덱스</param>
        /// <param name="count">설정할 호출 횟수</param>
        public void SetCardCallCount(int cardIndex, int count)
        {
            if (cardIndex >= 0 && cardIndex < cardCallCounts.Count)
                cardCallCounts[cardIndex] = count;
        }

        /// <summary>
        /// 특정 카드의 호출 횟수를 증가시킵니다.
        /// </summary>
        /// <param name="cardIndex">카드의 인덱스</param>
        /// <param name="increment">증가시킬 값 (기본값: 1)</param>
        public void IncrementCardCallCount(int cardIndex, int increment = 1)
        {
            if (cardIndex >= 0 && cardIndex < cardCallCounts.Count)
                cardCallCounts[cardIndex] += increment;
        }

        /// <summary>
        /// 카드 호출 순서 리스트를 반환합니다.
        /// </summary>
        /// <returns>카드 호출 순서 리스트</returns>
        public List<int> GetCallOrder() => cardCallOrder;

        /// <summary>
        /// 덱의 소유자를 반환합니다.
        /// </summary>
        /// <returns>덱을 소유한 캐릭터</returns>
        public Pawn GetOwner() => owner;
    }
}
