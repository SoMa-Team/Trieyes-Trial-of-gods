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
        // ===== 전투 시작/종료 관련 =====
        OnBattleStart,        // 전투 라운드가 시작되는 시점에 호출
        OnBattleClear,        // 전투 라운드가 클리어되는 시점에 호출
        OnBattleEnd,          // 전투 종료 이벤트
        OnBattleSceneChange,  // 전투 씬 생성 이전 -> 생성하려고 하는 즉시 호출되는 이벤트
        CalcActionInitOrder,  // 카드 액션 호출 순서 계산 이벤트
        DestoryCardsBeforeBattleStart, // 전투 시작 전 카드 파괴 이벤트
        
        // ===== 상시 호출 관련 =====
        OnTick,               // 전투 시작 후, 30TPS로 호출 (주기적인 감시가 필요한 데이터/동작에 사용)
        
        // ===== 전투 중 피격 관련 =====
        OnAttack,             // 적에게 공격이 성공한 경우 호출 (공격이 적용되기 직전)
        OnCriticalAttack,     // 적에게 치명타 공격이 성공할 경우 호출 (공격이 적용되기 직전)
        OnDamaged,            // 적에게 공격을 받을 경우 호출 (공격이 적용되기 직전)
        OnAttackHit,          // 적에게 공격 투사체가 닿았을 경우 호출 (회피/명중 판정 이전)
        OnDamageHit,          // 캐릭터에게 적 피사체가 닿았을 경우 호출 (회피/명중 판정 이전)
        OnAttackMiss,         // 공격이 회피당한 경우 호출 (공격자에게 발생)
        OnEvaded,             // 회피 성공 시 호출
        OnDefend,             // 공격 방어 시 호출되는 이벤트
        
        // ===== 전투 중 적 사망 관련 =====
        OnKilled,             // 적 처치시 호출 (공격이 적용된 뒤)
        OnKilledByCritical,   // 크리티컬 공격으로 적 처치시 호출 (공격이 적용된 뒤)
        OnDeath,              // 플레이어 사망시 호출 (게임 종료 판정 & 플레이어 사망 판정 직전)
        
        // ===== 스킬 관련 =====
        OnSkillCooldownEnd,   // 스킬 쿨타임 완료시 호출
        OnSkillInput,         // 플레이어가 스킬 버튼을 누를 경우 호출 (스킬 쿨다운 중에도 호출됨)
        
        // ===== 스탯 갱신 관련 =====
        OnHPUpdated,          // HP값이 갱신될 경우 호출 (preHP는 갱신 이전 HP값)
        OnGoldUpdated,        // 골드 소지량이 변할경우 호출 (preGold는 이전 골드량)  
        
        // ===== 기존 이벤트들 (하위 호환성) =====
        OnClear,              // 전투 클리어시 호출
        OnLevelUp,            // 레벨업 이벤트
        OnCardPurchase,       // 상점에서 카드 구매시 발동하는 이벤트
        OnCardRemove,         // 덱에서 카드 제거시 발동하는 이벤트
        
        OnRelicAdded,
        OnRelicRemoved,
        OnCardAdded,
        OnCardRemoved,
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
        abstract bool OnEvent(EventType eventType, object param);
    }
}