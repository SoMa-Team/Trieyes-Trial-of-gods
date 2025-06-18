namespace AttackComponents
{
    public static class AttackComponentFactory
    {
        // ===== [기능 1] AttackComponent 생성 =====
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