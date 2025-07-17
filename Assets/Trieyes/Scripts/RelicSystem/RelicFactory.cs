using System;
using System.Collections.Generic;
using System.Linq;
using AttackComponents;
using TagSystem;
using Unity.VisualScripting;
using UnityEngine;

namespace RelicSystem
{
    using RelicID = Int32;
    
    public class RelicFactory
    {
        public static Relic Create(RelicID relicID)
        {
            var relic = new Relic();

            RelicDataSO data = RelicDataBase.getRelicDataSO(relicID);
            
            relic.relicID = relicID;
            relic.name = data.name;
            relic.description = data.description;
            relic.filterAttackIDs = data.filterAttackIDs;
            relic.filterAttackTag = AttackTagManager.GetAttackTagByName(data.filterTag);
            relic.attackComponentIDs = data.attackComponentIDs;

            relic.randomOptions = new List<RandomOption>();
            
            // TODO: 실제 코드 주석 해제해야 함
            // for (int i = 0; i < getRandomOptionCount(relicID); i++)
            // {
            //     var randomOption = RandomOptionGenerator.Create(relicID);
            //     relic.randomOptions.Add(randomOption);
            // }
            
            // TODO : 테스트용 코드 지워야 함
            var randomOption = new RandomOption();
            randomOption.FilterTag = AttackTag.Range;
            randomOption.RelicStatType = RelicStatType.DamageIncreasement;
            randomOption.value = 10;
            relic.randomOptions.Add(randomOption);
            randomOption = new RandomOption();
            randomOption.FilterTag = AttackTag.Range;
            randomOption.RelicStatType = RelicStatType.ProjectileCount;
            randomOption.value = 2;
            relic.randomOptions.Add(randomOption);
            // TODO END : 테스트용 코드 끝
            
            return relic;
        }

        private static int getRandomOptionCount(RelicID id)
        {
            // TODO: 랜덤 옵션의 줄 수가 유동적일 경우 수정
            return 2;
        }
    }
} 