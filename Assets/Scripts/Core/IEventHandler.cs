using System;
using System.Collections.Generic;

namespace Core
{
    public enum EventType
    {
        OnProjectileKill,
        OnCriticalKill,
        OnDeath,
        OnClear,
        // ... 기타 이벤트
    }

    public delegate void EventDelegate(object param = null);

    public interface IEventHandler
    {
        void RegisterEvent(EventType eventType, EventDelegate handler);
        void UnregisterEvent(EventType eventType, EventDelegate handler);
        void TriggerEvent(EventType eventType, object param = null);
    }
} 