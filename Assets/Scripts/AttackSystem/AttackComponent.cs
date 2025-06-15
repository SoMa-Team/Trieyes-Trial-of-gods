using System.Collections.Generic;
using Core;

namespace AttackSystem
{
    /// <summary>
    /// 공격 관련 컴포넌트의 기본 동작을 정의하는 추상 클래스입니다.
    /// 이 컴포넌트는 IEventHandler를 구현하여 이벤트를 처리할 수 있습니다.
    /// </summary>
    public abstract class AttackComponent : IEventHandler
    {
        /// <summary>
        /// 이 AttackComponent 인스턴스에 등록된 이벤트 핸들러들을 관리하는 딕셔너리입니다.
        /// 각 EventType에 대해 여러 개의 EventDelegate를 가질 수 있습니다.
        /// </summary>
        private Dictionary<Core.EventType, List<EventDelegate>> _eventHandlers = new();

        public abstract void Execute(Attack attack);

        /// <summary>
        /// 특정 이벤트 타입에 대한 핸들러를 등록합니다.
        /// 이 AttackComponent가 해당 이벤트를 발동시켰을 때 handler 메서드가 호출됩니다.
        /// </summary>
        /// <param name="eventType">등록할 이벤트의 타입</param>
        /// <param name="handler">이벤트 발생 시 호출될 델리게이트 (메서드)</param>
        public virtual void RegisterEvent(Core.EventType eventType, EventDelegate handler)
        {
            if (!_eventHandlers.ContainsKey(eventType))
                _eventHandlers[eventType] = new List<EventDelegate>();
            _eventHandlers[eventType].Add(handler);
        }

        /// <summary>
        /// 특정 이벤트 타입에 등록된 핸들러를 해제합니다.
        /// 더 이상 해당 이벤트를 수신하지 않을 때 사용됩니다.
        /// </summary>
        /// <param name="eventType">해제할 이벤트의 타입</param>
        /// <param name="handler">해제할 델리게이트 (메서드)</param>
        public virtual void UnregisterEvent(Core.EventType eventType, EventDelegate handler)
        {
            if (_eventHandlers.ContainsKey(eventType))
                _eventHandlers[eventType].Remove(handler);
        }

        /// <summary>
        /// 특정 이벤트 타입에 대한 이벤트를 발동시킵니다.
        /// 이 AttackComponent가 이벤트를 발생시키는 역할을 합니다. 등록된 모든 핸들러들이 호출됩니다.
        /// </summary>
        /// <param name="eventType">발동시킬 이벤트의 타입</param>
        /// <param name="param">이벤트와 함께 전달될 추가 데이터 (선택 사항)</param>
        public virtual void TriggerEvent(Core.EventType eventType, object param = null)
        {
            if (_eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in _eventHandlers[eventType])
                    handler?.Invoke(param);
            }
        }

        /// <summary>
        /// 이 AttackComponent가 특정 이벤트에 반응할 때 호출되는 추상 메서드.
        /// 각 구체적인 AttackComponent 클래스에서 오버라이드하여 이벤트 로직을 구현합니다.
        /// </summary>
        /// <param name="eventType">발동된 이벤트 타입</param>
        /// <param name="param">이벤트 매개변수</param>
        public abstract void OnEvent(Core.EventType eventType, object param);
    }
} 