namespace CardActions
{
    public class Card0003_Haste : Card1001_GenericPositiveOnlyOnBattleStart
    {
        /// <summary>
        /// desc: 전투가 시작할 때, 이동속도를 10 증가시킵니다.
        /// </summary>
        public Card0003_Haste() : base(1, ParamKind.Add)
        {
        }
    }
}