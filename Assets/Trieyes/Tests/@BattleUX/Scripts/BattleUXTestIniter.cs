using System;
using System.Threading.Tasks;
using BattleSystem;
using CharacterSystem;
using RelicSystem;
using UnityEngine;
using Utils;

public class BattleUXTestIniter: BattleSystemTest
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _Start();
    }
    
    async Task _Start()
    {
        try
        {
            await RelicDataBase.InitializeAsync();
            var characterID = 0;
            var stageRound = 12;

            Pawn character = this.character;
        
            character.AddRelic(RelicFactory.Create(720001));
            character.AddRelic(RelicFactory.Create(720002));
            
            Difficulty difficulty = Difficulty.GetByStageRound(stageRound);
            BattleStage battleStage = BattleStageFactory.Instance.Create(character, difficulty);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
    }
}
