using Utils;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using CharacterSystem;
using CardSystem;

namespace CardActions
{
    public class Card0301_DestroyRightAndAbsorbAction : CardAction
{
    private const int statTypeIndex1 = 0; // 피해흡혈
    private const int statTypeIndex2 = 1; // 공격력
    private const int percentIndex   = 2; // 증가 퍼센트 (기본 30)

    public Card0301_DestroyRightAndAbsorbAction()
    {
        actionParams = new List<ActionParam>
        {
            // 피해흡혈(StatType)
            ActionParamFactory.Create(ParamKind.StatType, card =>
                StatTypeTransformer.KoreanToStatType(card.baseParams[statTypeIndex1])
            ),
            // 공격력(StatType)
            ActionParamFactory.Create(ParamKind.StatType, card =>
                StatTypeTransformer.KoreanToStatType(card.baseParams[statTypeIndex2])
            ),
            // 증가 퍼센트 (레벨 반영)
            ActionParamFactory.Create(ParamKind.Number, card =>
            {
                string raw = card.baseParams[percentIndex];
                int basePercent = int.Parse(raw);
                // 예시: 30 + 10 * 레벨
                return basePercent + 10 * card.cardEnhancement.level.Value;
            }),
        };
    }

    public override void OnEvent(Pawn owner, Deck deck, EventType eventType, object param)
    {
        if (eventType != EventType.OnBattleSceneChange) return;

        int myIdx = deck.Cards.IndexOf(card);
        if (myIdx != -1 && myIdx < deck.Cards.Count - 1)
        {
            var rightCard = deck.Cards[myIdx + 1];
            int destroyedLevel = rightCard.cardEnhancement.level.Value;
            deck.RemoveCard(rightCard);
            card.cardEnhancement.AddExp(destroyedLevel);
            card.RefreshStats();
        }

        // 파라미터 치환값 적용
        var statType1 = (StatType)GetEffectiveParam(statTypeIndex1);
        var statType2 = (StatType)GetEffectiveParam(statTypeIndex2);
        int percent = Convert.ToInt32(GetEffectiveParam(percentIndex));

        AddPercentBuff(owner, statType1, percent);
        AddPercentBuff(owner, statType2, percent);
    }

    private void AddPercentBuff(Pawn owner, StatType stat, int percent)
    {
        var baseValue = owner.statSheet[stat].BaseValue;
        int value = Mathf.RoundToInt(baseValue * (percent / 100f));
        var buff = new StatModifier(value, BuffOperationType.Additive);
        owner.statSheet[stat].AddBuff(buff);
        Debug.Log($"{stat}: {percent}% -> +{value} 버프");
    }
}

}