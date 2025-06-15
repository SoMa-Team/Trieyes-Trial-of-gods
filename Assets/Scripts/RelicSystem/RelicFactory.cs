using System.Collections.Generic;

namespace RelicSystem
{
    public class RelicFactory
    {
        private static Dictionary<int, System.Func<Relic>> relicCreators = new()
        {
            { 1, () => new Relic001() }
            // 다른 유물들도 여기에 추가
        };

        public static Relic CreateRelic(int relicId)
        {
            if (relicCreators.TryGetValue(relicId, out var creator))
            {
                return creator();
            }
            return null;
        }

        public static List<Relic> CreateRelics(List<int> relicIds)
        {
            var relics = new List<Relic>();
            foreach (var id in relicIds)
            {
                var relic = CreateRelic(id);
                if (relic != null)
                {
                    relics.Add(relic);
                }
            }
            return relics;
        }
    }
} 