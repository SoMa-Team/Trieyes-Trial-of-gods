using Stats;
using System.Collections.Generic;
using CharacterSystem;
using CardSystem;
using UnityEngine;
using Utils;

namespace CardActions
{
    /// <summary>
    /// 앉기(Crouch) 카드 액션을 구현하는 클래스입니다.
    /// OnBattleSceneChange 이벤트 발생 시 캐릭터의 방어력을 증가시킵니다.
    /// </summary>
    public class Crouch : CardAction
    {
        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            var descParams = param as string[];

            // 매개변수 유효성 검사
            if (owner == null || deck == null)
            {
                Debug.LogWarning("owner 또는 deck이 정의되지 않았습니다.");
                return;
            }
            if (descParams == null || descParams.Length < 2)
            {
                Debug.LogError("descParams null 또는 길이 부족");
                return;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                StatType statType = KoreanToStatType.ToStatType(descParams[0]);
                if (!int.TryParse(descParams[1], out int value))
                {
                    Debug.LogError($"descParams[1] 파싱 실패: {descParams[1]}");
                    return;
                }

                var modifier = new StatModifier(value, BuffOperationType.Additive);
                owner.statSheet[statType].AddBuff(modifier);

                Debug.Log($"<color=yellow>[Crouch] {statType} +{value}. New Value: {owner.statSheet[statType].Value}</color>");
            }
        }
    }
}