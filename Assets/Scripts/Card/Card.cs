using System.Collections.Generic;
using Core;

namespace Card
{
    public class Card : IEventHandler
    {
        public int cardId;
        public Rarity rarity;         // 카드 등급
        public int level;             // 카드 레벨
        public Property property;     // 카드 속성
        public CardStat stat;         // 카드 스탯
        public CardAction action;     // 카드 액션 (조건부, null 가능)

        // 이벤트 핸들러 구현
        private Dictionary<EventType, List<EventDelegate>> eventHandlers = new();
        public void RegisterEvent(EventType eventType, EventDelegate handler)
        {
            if (!eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType] = new List<EventDelegate>();
            eventHandlers[eventType].Add(handler);
        }
        public void UnregisterEvent(EventType eventType, EventDelegate handler)
        {
            if (eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType].Remove(handler);
        }
        public void TriggerEvent(EventType eventType, object param = null)
        {
            if (eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in eventHandlers[eventType])
                    handler?.Invoke(param);
            }
        }
    }

    public class CardStat
    {
        public float bonusAttack;
        public float bonusHp;
        // 기타 스탯
    }

    public enum Rarity { Common, Rare, Epic }
    public enum Property { Fire, Water, Earth, Air }
} 