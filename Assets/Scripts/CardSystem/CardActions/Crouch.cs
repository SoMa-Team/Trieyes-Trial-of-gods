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
        public StatType statType1 = StatType.Defense;
        public int baseValue = 10;

        public int calValue1(int cardLevel)
        {
            return baseValue * cardLevel;
        }

        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                // owner에서 card를 찾아오거나 param으로 Card를 넘길 수도 있음
                int cardLevel = param as int;
                int value1 = calValue1(cardLevel);

                owner.statSheet[statType1].AddBuff(new StatModifier(value1, BuffOperationType.Additive));
            }
        }

        public string[] GetDescriptionParams(Card card)
        {
            int level = card.cardEnhancement.level.Value;
            int value1 = baseValue * level;
            string koreanStat = StatTypeTransformer.ToStatType(statType1);
            return new string[] { "방어력", value1.ToString() };
        }
    }
}