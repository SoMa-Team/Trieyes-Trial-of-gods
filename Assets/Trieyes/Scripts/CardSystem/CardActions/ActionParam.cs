using CardSystem;
using System;

namespace CardActions
{
    public enum ParamKind
    {
        StatType,
        Add,
        Percent,
    }
    public class ActionParam
    {
        public ParamKind kind;
        public Func<Card, object> getBaseValue;
    }
}