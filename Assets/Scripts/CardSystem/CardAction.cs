using System.Collections.Generic;
using Core; // For IEventHandler and EventType
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 카드 액션의 기본 동작을 정의하는 추상 클래스입니다.
    /// 모든 구체적인 카드 액션은 이 클래스를 상속받아 고유한 OnEvent 로직을 구현해야 합니다.
    /// </summary>
    public abstract class CardAction : IEventHandler
    {
        public int cardId { get; private set; }
        public string cardDescription { get; protected set; }

        /// <summary>
        /// 이 CardAction 인스턴스에 등록된 이벤트 핸들러들을 관리하는 딕셔너리입니다.
        /// 각 EventType에 대해 여러 개의 EventDelegate를 가질 수 있습니다.
        /// </summary>
        private Dictionary<Core.EventType, List<EventDelegate>> eventHandlers = new();

        protected CardAction(int id, string description = "")
        {
            cardId = id;
            cardDescription = description;
        }

        /// <summary>
        /// 이벤트를 처리하는 추상 메서드입니다.
        /// 상속받는 클래스에서 특정 이벤트(예: OnAttack, OnDeath) 발생 시 수행할 고유한 로직을 구현합니다.
        /// </summary>
        /// <param name="eventType">발생한 이벤트의 타입</param>
        /// <param name="param">이벤트와 함께 전달된 매개변수</param>
        public abstract void OnEvent(Core.EventType eventType, object param);

        /// <summary>
        /// 특정 이벤트 타입에 대한 핸들러를 등록합니다.
        /// 이 CardAction이 해당 이벤트를 발동시켰을 때 handler 메서드가 호출됩니다.
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
        /// 이 CardAction이 이벤트를 발생시키는 역할을 합니다. 등록된 모든 핸들러들이 호출됩니다.
        /// (일반적으로 CardAction 자체에서 이벤트를 발동시키기보다는, Deck이나 Pawn으로부터 이벤트를 수신하여 OnEvent를 통해 처리합니다.)
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
    }
} 