namespace Card
{
    public static class CardActionFactory
    {
        public static CardAction CreateAction(int actionId)
        {
            switch (actionId)
            {
                case 1: return new CardAction001();
                case 2: return new CardAction002();
                // ... 기타 하드코딩
                default: return null;
            }
        }
    }
} 