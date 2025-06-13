namespace Relic
{
    public static class RelicFactory
    {
        public static Relic CreateRelic(int relicId)
        {
            // 하드코딩 or 데이터 기반 유물 생성
            return new Relic();
        }
    }
} 