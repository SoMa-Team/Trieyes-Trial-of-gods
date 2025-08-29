using System;
using CharacterSystem;
using RelicSystem;
using UnityEngine;
using Utils;

public class TestShopUXShopIniter : MonoBehaviour
{
    [SerializeField] private Character mainCharacter;

    private void Start()
    {
        var difficulty = Difficulty.GetByStageRound(1);
        mainCharacter.AddRelic(new Relic());
        mainCharacter.initBaseStat();
        mainCharacter.deck.Activate(mainCharacter, true);
        ShopSceneManager.Instance.Activate(mainCharacter, difficulty);
    }
}
