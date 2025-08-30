using CardSystem;
using System;

namespace CardActions
{
    public enum ParamKind {Number,StatType,Probability}
    public class ActionParam
    {
        public ParamKind kind;
        public Func<Card, object> getBaseValue;
    }
}