using BattleSystem;
using CharacterSystem;
using RelicSystem;
using UnityEngine;
using Utils;

public class RelicTestIniter: BattleSystemTest
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var characterID = 0;
        var stageRound = 12;

        Pawn character = this.character;
        
        character.basicAttack = attackData;
        Difficulty difficulty = Difficulty.GetByStageRound(stageRound);
        BattleStage battleStage = BattleStageFactory.Instance.Create(character, difficulty);
    }
}
