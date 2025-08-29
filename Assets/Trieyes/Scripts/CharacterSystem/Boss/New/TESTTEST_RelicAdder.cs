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
    }
}
