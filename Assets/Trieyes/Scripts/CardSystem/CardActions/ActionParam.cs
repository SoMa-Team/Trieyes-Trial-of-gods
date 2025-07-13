using CardSystem;
using System;

namespace CardActions
{
    public enum ParamKind {Number,StatType}
    public class ActionParam
    {
        public ParamKind kind;
        public Func<Card, object> getBaseValue;
    }
}