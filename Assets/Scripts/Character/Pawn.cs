using System.Collections.Generic;
using Core;

namespace Character
{
    public abstract class Pawn : IEventHandler
    {
        public Utils.StatInfo statInfo;
        private Dictionary<EventType, List<EventDelegate>> eventHandlers = new();

        public virtual void RegisterEvent(EventType eventType, EventDelegate handler)
        {
            if (!eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType] = new List<EventDelegate>();
            eventHandlers[eventType].Add(handler);
        }

        public virtual void UnregisterEvent(EventType eventType, EventDelegate handler)
        {
            if (eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType].Remove(handler);
        }

        public virtual void TriggerEvent(EventType eventType, object param = null)
        {
            if (eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in eventHandlers[eventType])
                    handler?.Invoke(param);
            }
        }

        public virtual void Update() { }
    }
} 