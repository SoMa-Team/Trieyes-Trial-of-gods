using System.Collections.Generic;
using Utils;
using UnityEngine;
using Stats;
using CardSystem;
using CharacterSystem;
using System;

namespace CardActions
{
    public class Card0602_Berserker : CardAction
    {
        private const int upStatTypeIdx  = 0;
        private const int upValueCoefIdx = 1;

        private StatModifier stat1Modifier;   // 같은 인스턴스 재사용(중첩 방지)
        private int perHitValue;              // 타격당 증가치(스티커/레벨 반영 후 고정)
        private BuffOperationType op;         // Additive 또는 Multiplicative

        public Card0602_Berserker()
        {
            actionParams = new List<ActionParam>
            {
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStatTypeIdx])),

                // 기본은 Add(정수 +N). Percent 스티커 붙이면 실행 시 자동으로 곱연산으로 전환됨.
                ActionParamFactory.Create(ParamKind.Add, card =>
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
                Debug.LogWarning("[Berserker] owner 또는 deck이 정의되지 않았습니다.");
                return false;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                // 스티커가 있으면 그 타입(Add/Percent) 우선, 없으면 ParamKind(Add)
                (perHitValue, op) = GetBuffFromParamPreferSticker(upValueCoefIdx);

                // 같은 StatModifier 인스턴스를 재사용하여 누적/중첩 관리
                // 전투 동안 지속(영구 취급), canStack: false 로 중첩 방지
                stat1Modifier = new StatModifier(0, op, canStack: false, duration: -1f);

                return true;
            }

            if (eventType == Utils.EventType.OnDamaged)
            {
                var statType = (StatType)GetEffectiveParam(upStatTypeIdx);

                // 누적치 갱신 (+N 또는 +N%를 누적)
                stat1Modifier.value += perHitValue;

                // 같은 인스턴스를 계속 추가 → 내부에서 중복 허용 안 하도록 구현되어 있으면 갱신 효과
                owner.statSheet[statType].AddBuff(stat1Modifier);
                return true;
            }

            return false;
        }
    }
}