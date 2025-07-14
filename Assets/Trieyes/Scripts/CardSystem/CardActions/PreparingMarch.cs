using UnityEngine;
using CharacterSystem;
using CardSystem;
using Stats;
using System.Collections.Generic;
using BattleSystem;
using Utils;
using System;

namespace CardActions
{
    /// <summary>
    /// 전진 준비(PreparingMarch) 카드 액션을 구현하는 클래스입니다.
    /// OnBattleSceneChange 이벤트 발생 시 캐릭터의 공격력을 증가시킵니다.
    /// </summary>
    public class PreparingMarch : CardAction
    {
        public PreparingMarch()
        {
            actionParams = new List<ActionParam>
            {
                // StatType, 예를 들어 baseParams[0]에 저장되어 있다고 가정
                ActionParamFactory.Create(ParamKind.StatType, card =>
                {
                    // 카드에 baseParams가 있다고 가정
                    // 예: "AttackPower" 등
                    // (Enum.Parse는 항상 예외처리 해주는 게 안전)
                    try
                    {
                        return (StatType)Enum.Parse(typeof(StatType), card.baseParams[0]);
                    }
                    catch
                    {
                        Debug.LogWarning("[PreparingMarch] baseParams[0] 파싱 실패, AttackPower로 대체");
                        return StatType.AttackPower;
                    }
                }),
                // 숫자, 예: baseParams[1]에 저장, * 레벨
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    int baseValue = 0;
                    try
                    {
                        baseValue = int.Parse(card.baseParams[1]);
                    }
                    catch
                    {
                        Debug.LogWarning("[PreparingMarch] baseParams[1] 파싱 실패, 10으로 대체");
                        baseValue = 10;
                    }
                    return baseValue * card.cardEnhancement.level.Value;
                }),
            };
        }

        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("owner 또는 deck이 정의되지 않았습니다.");
                return;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                // 이제 내부 card 필드만 사용!
                StatType statType = (StatType)GetEffectiveParam(0); // 인덱스 0: 스탯타입
                int value = Convert.ToInt32(GetEffectiveParam(1));  // 인덱스 1: 숫자

                var modifier = new StatModifier(value, BuffOperationType.Additive);
                owner.statSheet[statType].AddBuff(modifier);

                Debug.Log($"<color=yellow>[PreparingMarch] {statType} +{value}. New Value: {owner.statSheet[statType].Value}</color>");
            }
        }
    }
}