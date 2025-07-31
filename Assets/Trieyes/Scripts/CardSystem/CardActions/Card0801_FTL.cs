using System;
using Utils;

namespace CardActions
{
    public class Card0801_FTL : Card1002_StatBuffPositiveAndNegative
    {
        /// <summary>
        /// desc: 전투가 시작될 때, 해당 전투 동안 공격력 을/를 100% 감소시키고, 공격속도를 을/를 1000% 증가시킵니다.
        /// </summary>
        public Card0801_FTL() : base(true)
        {
            actionParams[1] = ActionParamFactory.Create(ParamKind.Number, card =>
            {
                int baseValue = Parser.ParseStrToInt(card.baseParams[1]);
                return baseValue;
            });
        }
    }
}