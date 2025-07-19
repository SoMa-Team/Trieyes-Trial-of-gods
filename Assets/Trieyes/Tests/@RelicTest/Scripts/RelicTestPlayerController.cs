using System;
using CharacterSystem;
using RelicSystem;
using UnityEngine;

public class RelicTestPlayerController : PlayerController
{
    private void Update()
    {
        owner.PerformAutoAttack();
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            owner.AddRelic(RelicFactory.Create(0));
        }
    }
}
