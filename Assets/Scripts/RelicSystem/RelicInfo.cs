using System.Collections.Generic;

namespace RelicSystem
{
    public class RelicInfo
    {
        // ===== [기능 1] 유물 정보 =====
        public string name;
        public int relicId;
        public List<int> compIds = new List<int>(); // 이 유물이 영향을 주는 AttackData의 ID 목록
    }
} 