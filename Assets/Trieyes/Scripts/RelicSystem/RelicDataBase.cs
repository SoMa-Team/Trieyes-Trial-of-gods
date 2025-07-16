using System.Collections.Generic;
using NUnit.Framework;
using TagSystem;
using RelicID = System.Int32;

namespace RelicSystem
{
    public class RelicDataBase
    {
        public RelicDataSO[] RelicData;
        
        public static RelicDataSO getRelicDataSO(int id)
        {
            // TODO: ID에 따라 RelicDataSO를 선택하는 로직 필요
            var result = RelicDataSO.CreateInstance<RelicDataSO>();
            result.id = id;
            result.name = "용사의 창";
            result.description = "**찌르기**의 사거라가 100%증가합니다.";
            result.filterAttackIDs = null;
            result.filterTag = "range";
            result.attackComponentIDs = new List<int>();
            result.attackComponentIDs.Add(0);
            return result;
        }
    }
}