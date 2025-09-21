using System.Collections.Generic;
using Utils;
using UnityEngine;
using Stats;
using CardSystem;
using CharacterSystem;
using System;

namespace CardActions
{
    public class Card0802_RageOfBlade : CardAction
    {
        private const int downStatTypeIdx  = 0;
        private const int downValueCoefIdx = 1;
        private const int upStatTypeIdx    = 2;
        private const int upValueCoefIdx   = 3;

        private StatModifier upModifier;     // 같은 인스턴스 재사용(중첩 방지)
        private int perHitValue;             // 1회 공격당 증가량
        private BuffOperationType upOp;      // Additive or Multiplicative

        public Card0802_RageOfBlade()
        {
            actionParams = new List<ActionParam>
            {
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[downStatTypeIdx])),
                
                ActionParamFactory.Create(ParamKind.Percent, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[downValueCoefIdx]);
                    return baseValue;
                }),
                
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStatTypeIdx])),
                
                ActionParamFactory.Create(ParamKind.Percent, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[upValueCoefIdx]);
                    return baseValue * card.cardEnhancement.level.Value;
                }),
            };
        }

        public override bool OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("[RageOfBlade] owner 또는 deck이 정의되지 않았습니다.");
                return false;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                // 시작 디버프: 스티커가 있으면 스티커 타입(op) 우선
                var downStat = (StatType)GetEffectiveParam(downStatTypeIdx);
                var (downVal, downOp) = GetBuffFromParamPreferSticker(downValueCoefIdx);
                downVal = -downVal; // 감소이므로 음수

                owner.statSheet[downStat].AddBuff(new StatModifier(downVal, downOp, canStack: false, duration: -1f));

                // 증가 버프 준비: 같은 인스턴스 재사용해서 중첩 없이 누적
                (perHitValue, upOp) = GetBuffFromParamPreferSticker(upValueCoefIdx);
                upModifier = new StatModifier(0, upOp, canStack: false, duration: -1f);

                return true;
            }

            if (eventType == Utils.EventType.OnAttack)
            {
                var upStat = (StatType)GetEffectiveParam(upStatTypeIdx);

                // 누적량 업데이트(+N 또는 +N%)
                upModifier.value += perHitValue;

                owner.statSheet[upStat].AddBuff(upModifier);
                return true;
            }

            return false;
        }
    }
}