using System;

namespace CardActions
{
    public class Card0801_FTL : Card1002_StatBuffPositiveAndNegative
    {
        public Card0801_FTL() : base(false)
        {
            actionParams[1] = ActionParamFactory.Create(ParamKind.Number, card =>
            {
                string raw = card.baseParams[1];
                if (!int.TryParse(raw, out int baseValue))
                    throw new InvalidOperationException($"[GenericStatBuff] baseParams[{1}] 변환 실패: {raw}");
                return baseValue;
            });
        }
    }
}