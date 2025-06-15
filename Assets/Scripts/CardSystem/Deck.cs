using System.Collections.Generic;
using Core;
using Utils;
using System.Linq;
using UnityEngine;

namespace CardSystem
{
    public class Deck : IEventHandler
    {
        public List<Card> cards = new List<Card>();

        /// <summary>
        /// 이 Deck 인스턴스에 등록된 이벤트 핸들러들을 관리하는 딕셔너리입니다.
        /// 각 EventType에 대해 여러 개의 EventDelegate를 가질 수 있습니다.
        /// </summary>
        private Dictionary<Core.EventType, List<EventDelegate>> eventHandlers = new();

        public Deck()
        {
            // 생성자, 필요에 따라 초기화 로직 추가
        }

        public void AddCard(Card card)
        {
            if (card != null)
            {
                cards.Add(card);
            }
        }

        public void RemoveCard(Card card)
        {
            if (card != null)
            {
                cards.Remove(card);
            }
        }

        /// <summary>
        /// 덱의 모든 카드 스탯을 합산하여 반환합니다.
        /// 이 합산된 스탯은 Pawn의 기본 스탯(statinfos)에 적용됩니다.
        /// </summary>
        /// <returns>합산된 StatInfo 리스트</returns>
        public List<StatInfo> CalcBaseStat()
        {
            List<StatInfo> totalDeckStats = new List<StatInfo>();
            foreach (var card in cards)
            {
                if (card != null)
                {
                    totalDeckStats = StatCalculator.AddStats(totalDeckStats, card.GetAllCardStatInfos());
                }
            }
            return totalDeckStats;
        }

        /// <summary>
        /// 덱의 모든 카드를 순회하며 특정 이벤트 타입에 해당하는 CardAction의 OnEvent를 발동합니다.
        /// 이 메서드는 Pawn으로부터 특정 이벤트 (예: 사망)가 발생했음을 전달받아 덱 내의 카드 액션들이 반응하도록 합니다.
        /// </summary>
        /// <param name="eventType">발동할 이벤트 타입 (예: Core.EventType.OnBattleStart)</param>
        /// <param name="param">이벤트 매개변수</param>
        public void CalcActionInitStat(Core.EventType eventType, object param = null)
        {
            foreach (var card in cards)
            {
                if (card != null && card.cardAction != null)
                {
                    card.TriggerCardEvent(eventType, param);
                }
            }
        }

        /// <summary>
        /// 특정 이벤트 타입에 대한 핸들러를 등록합니다.
        /// 이 Deck 인스턴스가 해당 이벤트를 발동시켰을 때 handler 메서드가 호출됩니다.
        /// </summary>
        /// <param name="eventType">등록할 이벤트의 타입</param>
        /// <param name="handler">이벤트 발생 시 호출될 델리게이트 (메서드)</param>
        public virtual void RegisterEvent(Core.EventType eventType, EventDelegate handler)
        {
            if (!eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType] = new List<EventDelegate>();
            eventHandlers[eventType].Add(handler);
        }

        /// <summary>
        /// 특정 이벤트 타입에 등록된 핸들러를 해제합니다.
        /// 더 이상 해당 이벤트를 수신하지 않을 때 사용됩니다.
        /// </summary>
        /// <param name="eventType">해제할 이벤트의 타입</param>
        /// <param name="handler">해제할 델리게이트 (메서드)</param>
        public virtual void UnregisterEvent(Core.EventType eventType, EventDelegate handler)
        {
            if (eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType].Remove(handler);
        }

        /// <summary>
        /// 특정 이벤트 타입에 대한 이벤트를 발동시킵니다.
        /// 이 Deck이 이벤트를 발생시키는 역할을 합니다. 등록된 모든 핸들러들이 호출됩니다.
        /// </summary>
        /// <param name="eventType">발동시킬 이벤트의 타입</param>
        /// <param name="param">이벤트와 함께 전달될 추가 데이터 (선택 사항)</param>
        public virtual void TriggerEvent(Core.EventType eventType, object param = null)
        {
            if (eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in eventHandlers[eventType])
                    handler?.Invoke(param);
            }
        }

        /// <summary>
        /// Deck 자체에서 이벤트를 처리할 때 호출됩니다.
        /// 이 이벤트를 CardAction으로 전파하거나, Deck에 특정한 로직을 수행할 수 있습니다.
        /// (이 메서드는 Deck이 IEventHandler로서 외부에 자신의 이벤트를 노출할 때 사용됩니다.)
        /// </summary>
        /// <param name="eventType">발동된 이벤트 타입</param>
        /// <param name="param">이벤트 매개변수</param>
        public virtual void OnEvent(Core.EventType eventType, object param)
        {
            // 예시: Deck 관련 특정 이벤트 처리
            if (eventType == Core.EventType.OnBattleStart) 
            {
                Debug.Log($"Deck: 전투 시작 이벤트 수신! 카드 액션 초기화.");
                // 전투 시작 시 모든 카드의 액션을 초기화 (CalcActionInitStat) 할 수 있습니다.
                // CalcActionInitStat(eventType, param); // 이미 Pawn에서 호출하므로 여기서는 중복 호출 피함
            }
            // 다른 이벤트 처리 로직 추가
        }
    }
} 