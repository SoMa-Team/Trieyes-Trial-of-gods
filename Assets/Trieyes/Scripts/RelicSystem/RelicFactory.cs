using System;
using System.Collections.Generic;
using System.Linq;
using AttackComponents;
using RelicSystem.RelicActions;
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

            relic.relicAction = relicID switch
            {
                101 => new Relic0101_SummonPet(),
                102 => new Relic0102_Perfectionist(),
                103 => new Relic0103_Stop(),
                104 => new Relic0104_HeroSword(),
                105 => new Relic0105_HeroShield(),
                _ => null
            };
            relic.relicAction?.AttachTo(relic);
        
            return relic;
        }
    }
} 