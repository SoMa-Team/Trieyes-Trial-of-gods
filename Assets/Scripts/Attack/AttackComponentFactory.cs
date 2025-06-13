namespace Attack
{
    public static class AttackComponentFactory
    {
        public static AttackComponent CreateComponent(int compId)
        {
            switch (compId)
            {
                case 1: return new AttackComponent001();
                case 2: return new AttackComponent002();
                // ... 기타 하드코딩
                default: return null;
            }
        }
    }
} 