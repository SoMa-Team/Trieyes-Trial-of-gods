using UnityEngine;
using CharacterSystem;
using CardSystem;
using Stats;
using System.Collections.Generic;
using BattleSystem;

namespace CardActions
{
    /// <summary>
    /// 전진 준비(PreparingMarch) 카드 액션을 구현하는 클래스입니다.
    /// OnBattleSceneChange 이벤트 발생 시 캐릭터의 공격력을 증가시킵니다.
    /// 전투 준비 단계에서 공격력을 강화하는 전략적 카드입니다.
    /// </summary>
    public class PreparingMarch : CardAction
    {
        // --- 필드 ---

        /// <summary>
        /// 이 카드가 OnBattleSceneChange 이벤트에서 제공하는 공격력 증가 수치입니다.
        /// 기본값은 10으로 설정되어 있으며, 캐릭터의 공격력을 이만큼 증가시킵니다.
        /// </summary>
        public int attackPowerIncrease = 10;

        // --- public 메서드 ---

        /// <summary>
        /// 카드 액션이 이벤트에 반응할 때 호출되는 메서드입니다.
        /// OnBattleSceneChange 이벤트 발생 시 캐릭터의 공격력을 증가시킵니다.
        /// 전투 장면이 변경될 때마다 공격력 버프를 적용합니다.
        /// </summary>
        /// <param name="owner">카드를 소유한 캐릭터</param>
        /// <param name="deck">카드가 속한 덱</param>
        /// <param name="eventType">발생한 이벤트 타입</param>
        /// <param name="param">이벤트와 함께 전달된 매개변수</param>
        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            // 매개변수 유효성 검사
            if (owner == null || deck == null)
            {
                Debug.LogWarning("owner 또는 deck이 정의되지 않았습니다.");
                return;
            }

            // OnBattleSceneChange 이벤트 처리
            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                Debug.Log($"PreparingMarch.OnEvent: OnBattleSceneChange");
                
                // 공격력 증가 버프 생성 및 적용
                var modifier = new StatModifier(attackPowerIncrease, BuffOperationType.Additive);
                Debug.Log("modifier Created");
                owner.statSheet[StatType.AttackPower].AddBuff(modifier);
                Debug.Log($"<color=yellow>[PreparingMarch] ATK +{attackPowerIncrease}. New Value: {owner.statSheet[StatType.AttackPower].Value}</color>");
            }
        }
    }
} 