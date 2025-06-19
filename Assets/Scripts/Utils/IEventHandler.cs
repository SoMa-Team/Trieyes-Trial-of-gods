using System;
using System.Collections.Generic;

namespace Utils
{
    // ===== [기능 1] 이벤트 델리게이트
    
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
        OnCardPurchase, // 상점에서 카드 구매시 발동하는 이벤트
        // ... 기타 이벤트
    }

    /// <summary>
    /// 이벤트 발생 시 호출될 메서드의 시그니처를 정의하는 델리게이트입니다.
    /// 이벤트를 수신하는 모든 핸들러는 이 델리게이트 타입에 맞춰 메서드를 구현해야 합니다.
    /// </summary>
    /// <param name="param">이벤트와 함께 전달되는 추가 데이터 (없을 경우 null)</param>
    public delegate void EventDelegate(object param = null);

    // ===== [기능 3] 이벤트 핸들러 인터페이스 =====
    public interface IEventHandler
    {
        abstract void OnEvent(EventType eventType, object param);
    }
}