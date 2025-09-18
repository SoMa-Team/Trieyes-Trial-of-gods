using System;
using Utils;

namespace CardActions
{
    public class Card0801_FTL : Card1002_StatBuffPositiveAndNegative
    {
        /// <summary>
        /// desc: 전투가 시작될 때, 해당 전투 동안 공격력을 100% 감소(-)시키고,
        ///       공격속도를 1000% 증가(+)시킵니다. (값은 레벨 스케일 없음)
        /// </summary>
        public Card0801_FTL() : base(ParamKind.Percent)
        {
            // value[0] (index 1): 첫 번째 쌍의 값(부모에서 음수로 적용됨). 레벨 스케일 제거
            actionParams[1] = ActionParamFactory.Create(ParamKind.Percent, card =>
            {
                int baseValue = Parser.ParseStrToInt(card.baseParams[1]); // 예: 100
                return baseValue;
            });

            // value[1] (index 3): 두 번째 쌍의 값. 레벨 스케일 제거
            actionParams[3] = ActionParamFactory.Create(ParamKind.Percent, card =>
            {
                int baseValue = Parser.ParseStrToInt(card.baseParams[3]); // 예: 1000
                return baseValue;
            });
        }
    }
}