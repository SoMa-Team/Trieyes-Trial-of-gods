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
            switch (id)
            {
                case 0:
                    result.name = "화염 전용";
                    result.description = "[화염] 공격은 적용되지 않아야 합니다!";
                    result.filterAttackIDs = null;
                    result.filterTag = "fire";
                    result.attackComponentIDs = new List<int>();
                    break;
                
                case 1:
                    result.name = "용사의 창";
                    result.description = "[범위] 공격의 사거리가 100%증가합니다.";
                    result.filterAttackIDs = null;
                    result.filterTag = "range";
                    result.attackComponentIDs = new List<int>();
                    result.attackComponentIDs.Add(0);
                    break;
            }
            
            return result;
        }
    }
}