using System.Collections.Generic;
using Core;
using Utils;

namespace RelicSystem
{
    /// <summary>
    /// 게임 내 유물을 나타내는 클래스입니다.
    /// 유물은 자체적으로 이벤트를 등록하고 처리할 수 있는 IEventHandler를 구현합니다.
    /// </summary>
    public class Relic : IEventHandler
    {
        public RelicInfo info;
        /// <summary>
        /// 이 Relic 인스턴스에 등록된 이벤트 핸들러들을 관리하는 딕셔너리입니다.
        /// 각 EventType에 대해 여러 개의 EventDelegate를 가질 수 있습니다.
        /// </summary>
        private Dictionary<EventType, List<EventDelegate>> eventHandlers = new();

        public Relic(RelicInfo info)
        {
            this.info = info;
        }

        /// <summary>
        /// 이 Relic이 특정 이벤트에 반응할 때 호출됩니다.
        /// 등록된 이벤트 핸들러들을 통해 이벤트를 실제로 발동시킵니다.
        /// </summary>
        /// <param name="type">발동된 이벤트의 타입</param>
        /// <param name="obj">이벤트와 함께 전달된 매개변수</param>
        public virtual void OnEvent(EventType type, object obj)
        {
            if (eventHandlers.ContainsKey(type))
            {
                foreach (var handler in eventHandlers[type])
                    handler?.Invoke(obj);
            }
        }

        /// <summary>
        /// 특정 이벤트 타입에 대한 핸들러를 등록합니다.
        /// 이 Relic이 해당 이벤트를 발동시켰을 때 handler 메서드가 호출됩니다.
        /// </summary>
        /// <param name="eventType">등록할 이벤트의 타입</param>
        /// <param name="handler">이벤트 발생 시 호출될 델리게이트 (메서드)</param>
        public virtual void RegisterEvent(EventType eventType, EventDelegate handler)
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
        public virtual void UnregisterEvent(EventType eventType, EventDelegate handler)
        {
            if (eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType].Remove(handler);
        }

        /// <summary>
        /// 특정 이벤트 타입에 대한 이벤트를 발동시킵니다.
        /// 이 Relic이 이벤트를 발생시키는 역할을 합니다. 등록된 모든 핸들러들이 호출됩니다.
        /// (Relic에서 직접 이벤트를 발동하는 경우에 사용됩니다.)
        /// </summary>
        /// <param name="eventType">발동시킬 이벤트의 타입</param>
        /// <param name="param">이벤트와 함께 전달될 추가 데이터 (선택 사항)</param>
        public virtual void TriggerEvent(EventType eventType, object param = null)
        {
            if (eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in eventHandlers[eventType])
                    handler?.Invoke(param);
            }
        }
    }
} 