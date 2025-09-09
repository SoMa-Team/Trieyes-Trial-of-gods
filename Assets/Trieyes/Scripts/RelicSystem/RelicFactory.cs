using System;
using System.Collections.Generic;
using System.Linq;
using AttackComponents;
using TagSystem;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace RelicSystem
{
    using RelicID = Int32;
    
    public class RelicFactory
    {
        public static Relic Create(RelicID relicID)
        {
            var relic = new Relic();
            
            Debug.Log($"Relic ID: {relicID}");

            RelicDataSO data = RelicDataBase.GetRelicDataSO(relicID);
            if (data == null)
            {
                Debug.LogError($"Relic ID: {relicID} not found");
            }
            relic.relicID = relicID;
            relic.name = data.name;
            relic.icon = data.icon;
            relic.description = data.description;
            relic.filterAttackIDs = data.filterAttackIDs;
            relic.attackComponentIDs = data.attackComponentIDs;
            
        // public int relicID;
        // public string name;
        // public Sprite icon = null;
        // public string description;
        //
        // public RelicAction relicAction;
        // // Relic의 이벤트 Handler
        // public List<int> filterAttackIDs;
        // // 유물이 적용되는 공격 대상 // null일 경우 전체 공격 대상
        // public List<int> attackComponentIDs;
        //     // 유물이 적용되는 공격에 부착되는 AttackComponent
        
            return relic;
        }
    }
} 