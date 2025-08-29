using System.Collections.Generic;
using BattleSystem;
using CharacterSystem;
using RelicSystem;
using TagSystem;
using UnityEngine;

public class TESTTEST_RelicAdder : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            var relic = new Relic();
            var relicRandomOption = new RandomOption
            {
                FilterTag = AttackTag.Water,
                RelicStatType = RelicStatType.ProjectileCount,
                value = 1
            };
            
            relic.randomOptions = new List<RandomOption> { relicRandomOption };
            
            var character = BattleStage.now.mainCharacter;
            CharacterFactory.Instance.Deactivate(character);
            BattleStage.now.mainCharacter.AddRelic(relic);
            CharacterFactory.Instance.Activate(character);
        }
        
        if (Input.GetKeyDown(KeyCode.X))
        {
            var relic = new Relic();
            var relicRandomOption = new RandomOption
            {
                FilterTag = AttackTag.Water,
                RelicStatType = RelicStatType.ProjectilePierce,
                value = 1
            };
            
            relic.randomOptions = new List<RandomOption> { relicRandomOption };
            
            var character = BattleStage.now.mainCharacter;
            CharacterFactory.Instance.Deactivate(character);
            BattleStage.now.mainCharacter.AddRelic(relic);
            CharacterFactory.Instance.Activate(character);
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            var relic = new Relic();
            var relicRandomOption = new RandomOption
            {
                FilterTag = AttackTag.Water,
                RelicStatType = RelicStatType.Range,
                value = 10
            };
            
            relic.randomOptions = new List<RandomOption> { relicRandomOption };
            
            var character = BattleStage.now.mainCharacter;
            CharacterFactory.Instance.Deactivate(character);
            BattleStage.now.mainCharacter.AddRelic(relic);
            CharacterFactory.Instance.Activate(character);
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        {
            var relic = new Relic();
            var relicRandomOption = new RandomOption
            {
                FilterTag = AttackTag.Water,
                RelicStatType = RelicStatType.AOE,
                value = 10
            };
            
            relic.randomOptions = new List<RandomOption> { relicRandomOption };
            
            var character = BattleStage.now.mainCharacter;
            CharacterFactory.Instance.Deactivate(character);
            BattleStage.now.mainCharacter.AddRelic(relic);
            CharacterFactory.Instance.Activate(character);
        }
    }
}
