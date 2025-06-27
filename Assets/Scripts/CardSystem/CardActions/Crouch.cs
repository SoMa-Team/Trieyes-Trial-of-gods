using Stats;
using System.Collections.Generic;
using CharacterSystem;
using CardSystem;
using UnityEngine;

namespace CardActions
{
    public class Crouch : CardAction
    {
        // --- 필드 ---
        public int defensePowerIncrease=10;

        // --- public 메서드 ---

        /// <summary>
        /// 카드 액션이 이벤트에 반응할 때 호출되는 메서드입니다.
        /// OnBattleSceneChange 이벤트 발생 시 캐릭터의 방어력을 증가시킵니다.
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
                // 방어력 증가 버프 생성 및 적용
                var modifier = new StatModifier(defensePowerIncrease, BuffOperationType.Additive);
                owner.statSheet[StatType.Defense].AddBuff(modifier);
                Debug.Log($"<color=yellow>[Crouch] DEF +{defensePowerIncrease}. New Value: {owner.statSheet[StatType.Defense].Value}</color>");
            }
        }
    }
}