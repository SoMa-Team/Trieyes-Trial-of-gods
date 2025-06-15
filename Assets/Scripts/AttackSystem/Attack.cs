using System.Collections.Generic;
using CharacterSystem;
using Core;
using UnityEngine;

namespace AttackSystem
{
    /// <summary>
    /// 게임 내 공격 행위를 정의하는 클래스입니다.
    /// 이 클래스는 IEventHandler를 구현하여 자체적으로 이벤트를 처리하고 발동시킬 수 있습니다.
    /// </summary>
    public class Attack : IEventHandler
    {
        public AttackData attackData;
        public Pawn attacker;
        public List<AttackComponent> components = new List<AttackComponent>();

        /// <summary>
        /// 이 Attack 인스턴스에 등록된 이벤트 핸들러들을 관리하는 딕셔너리입니다.
        /// 각 EventType에 대해 여러 개의 EventDelegate를 가질 수 있습니다.
        /// </summary>
        private Dictionary<Core.EventType, List<EventDelegate>> eventHandlers = new();

        public Attack(AttackData data)
        {
            attackData = data;
            // attackComponents 초기화 로직 (외부에서 주입될 수도 있음)
        }

        public void Activate()
        {
            Debug.Log("Attack Activated!");
            // 공격 활성화 로직
        }

        public void Deactivate()
        {
            Debug.Log("Attack Deactivated!");
            // 공격 비활성화 로직
        }

        /// <summary>
        /// 특정 이벤트 타입에 대한 핸들러를 등록합니다.
        /// 이 Attack이 해당 이벤트를 발동시켰을 때 handler 메서드가 호출됩니다.
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
        /// 이 Attack이 이벤트를 발생시키는 역할을 합니다. 등록된 모든 핸들러들이 호출됩니다.
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
        /// Attack 클래스 자체에서 이벤트를 처리할 때 호출됩니다.
        /// 예를 들어, 특정 이벤트 발생 시 공격력을 변경하거나, 특정 컴포넌트를 활성화/비활성화 할 수 있습니다.
        /// </summary>
        /// <param name="eventType">발동된 이벤트 타입</param>
        /// <param name="param">이벤트 매개변수</param>
        public virtual void OnEvent(Core.EventType eventType, object param)
        {
            // 예시: 공격 관련 특정 이벤트 처리
            if (eventType == Core.EventType.OnBattleEnd)
            {
                Debug.Log($"Attack: 전투 종료 이벤트 수신! 공격 비활성화.");
                Deactivate();
            }
            else if (eventType == Core.EventType.OnDeath)
            {
                if (param is CharacterSystem.Pawn deadPawn)
                {
                    Debug.Log($"Attack: {deadPawn.gameObject.name} 사망 이벤트 수신. 특정 공격 버프 제거.");
                    // 사망한 대상에 따른 공격 관련 로직 구현
                }
            }
            // 다른 이벤트 처리 로직 추가
        }

        public void Execute()
        {
            foreach (var comp in components)
                comp.Execute(this);
        }
    }
} 