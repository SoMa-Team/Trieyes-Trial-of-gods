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
            relic.relicType = data.relicType;
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
                106 => new Relic0106_SnipersEye(),
                107 => new Relic0107_FlagOfBattleField(),
                108 => new Relic0108_MirrorOfCurse(),
                109 => new Relic0109_GoldenLeaf(),
                110 => new Relic0110_JudgesSword(),
                111 => new Relic0111_Swiftheart(),
                112 => new Relic0112_TwistedBoots(),
                113 => new Relic0113_FountainOfLife(),
                114 => new Relic0114_WingedBoots(),
                115 => new Relic0115_HuntersInstinct(),
                116 => new Relic0116_ChaliceOfReversal(),
                117 => new Relic0117_BloodOath(),
                118 => new Relic0118_ChainsOfEndurance(),
                119 => new Relic0119_CrystalOrb(),
                120 => new Relic0120_Pinball(),
                _ => null
            };
            relic.relicAction?.AttachTo(relic);
        
            return relic;
        }
    }
} 