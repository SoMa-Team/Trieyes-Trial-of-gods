namespace CardActions
{
    public class Card0002_Crouch : Card1001_GenericPositiveOnlyOnBattleStart
    {  
        /// <summary>
        /// desc: 전투가 시작할 때, 방어력을 10 증가시킵니다.
        /// </summary>
        public Card0002_Crouch() : base(1, ParamKind.Add)
        {
            
        }
    }
}