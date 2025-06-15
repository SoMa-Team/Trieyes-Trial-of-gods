using System;
using System.Collections.Generic;

namespace Core
{
    /// <summary>
    /// 게임 내 이벤트 타입을 정의합니다.
    /// 각 이벤트 타입은 특정 상황이나 상태 변화를 나타냅니다.
    /// </summary>
    public enum EventType
    {
        OnProjectileKill,
        OnCriticalKill,
        OnDeath,
        OnClear,
        OnLevelUp,    // 레벨업 이벤트
        OnStatChange, // 스탯 변경 이벤트
        OnBattleSceneChange, // 전투 씬 생성 이전 -> 생성하려고 하는 즉시 호출되는 이벤트
        OnBattleStart, // 전투 씬 생성 이후 시작 시 호출 되는 이벤트
        OnAttack, // 공격 명령 시 호출 되는 이벤트
        OnAttackHit, // 공격 명중 시 호출 되는 이벤트
        OnDefend, // 공격 방어 시 호출 되는 이벤트
        OnHit,    // 피격 이벤트
        OnBattleEnd,    // 전투 종료 이벤트
        // ... 기타 이벤트
    }

    /// <summary>
    /// 이벤트 발생 시 호출될 메서드의 시그니처를 정의하는 델리게이트입니다.
    /// 이벤트를 수신하는 모든 핸들러는 이 델리게이트 타입에 맞춰 메서드를 구현해야 합니다.
    /// </summary>
    /// <param name="param">이벤트와 함께 전달되는 추가 데이터 (없을 경우 null)</param>
    public delegate void EventDelegate(object param = null);

    /// <summary>
    /// 게임 내 이벤트 발행/구독 모델을 위한 인터페이스입니다.
    /// 이 인터페이스를 구현하는 클래스는 이벤트를 등록하고, 해제하며, 발동시킬 수 있습니다.
    /// </summary>
    public interface IEventHandler
    {
        /// <summary>
        /// 특정 이벤트 타입에 대한 핸들러를 등록합니다.
        /// 이 메서드를 통해 해당 이벤트가 발생했을 때 호출될 함수를 시스템에 알립니다.
        /// </summary>
        /// <param name="eventType">등록할 이벤트의 타입</param>
        /// <param name="handler">이벤트 발생 시 호출될 델리게이트 (메서드)</param>
        void RegisterEvent(EventType eventType, EventDelegate handler);

        /// <summary>
        /// 특정 이벤트 타입에 등록된 핸들러를 해제합니다.
        /// 더 이상 해당 이벤트를 수신하지 않을 때 사용됩니다.
        /// </summary>
        /// <param name="eventType">해제할 이벤트의 타입</param>
        /// <param name="handler">해제할 델리게이트 (메서드)</param>
        void UnregisterEvent(EventType eventType, EventDelegate handler);

        /// <summary>
        /// 특정 이벤트 타입에 대한 이벤트를 발동시킵니다.
        /// 이 메서드가 호출되면, 해당 이벤트 타입에 등록된 모든 핸들러들이 호출됩니다.
        /// </summary>
        /// <param name="eventType">발동시킬 이벤트의 타입</param>
        /// <param name="param">이벤트와 함께 전달될 추가 데이터 (선택 사항)</param>
        void TriggerEvent(EventType eventType, object param = null);
    }
} 