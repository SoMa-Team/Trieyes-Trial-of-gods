using CardSystem;
using System;

namespace CardActions
{
    public static class ActionParamFactory
    {
        public static ActionParam Create(ParamKind kind, Func<Card, object> func)
            => new ActionParam { kind = kind, getBaseValue = func };
    }
}