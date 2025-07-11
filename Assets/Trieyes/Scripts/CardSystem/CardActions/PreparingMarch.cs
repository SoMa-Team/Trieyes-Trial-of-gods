using UnityEngine;
using CharacterSystem;
using CardSystem;
using Stats;
using System.Collections.Generic;
using BattleSystem;
using Utils;

namespace CardActions
{
    /// <summary>
    /// 전진 준비(PreparingMarch) 카드 액션을 구현하는 클래스입니다.
    /// OnBattleSceneChange 이벤트 발생 시 캐릭터의 공격력을 증가시킵니다.
    /// </summary>
    public class PreparingMarch : CardAction
    {
        public StatType statType1 = StatType.AttackPower;
        public int baseValue = 10;

        public int CalValue1(int cardLevel)
        {
            return baseValue * cardLevel;
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
                Card card = param as Card;
                Debug.Log($"card: {card?.cardName}");
                int level = card?.cardEnhancement.level.Value ?? 1;
                int value1 = CalValue1(level);

                Debug.Log($"level: {level}, value1: {value1}");

                var modifier = new StatModifier(value1, BuffOperationType.Additive);
                owner.statSheet[statType1].AddBuff(modifier);

                Debug.Log($"<color=yellow>[PreparingMarch] {statType1} +{value1}. New Value: {owner.statSheet[statType1].Value}</color>");
            }
        }

        public override string[] GetDescriptionParams(Card card)
        {
            int level = card.cardEnhancement.level.Value;
            int value1 = CalValue1(level);
            string koreanStat = StatTypeTransformer.StatTypeToKorean(statType1);
            return new string[] { koreanStat, value1.ToString() };
        }
    }
}