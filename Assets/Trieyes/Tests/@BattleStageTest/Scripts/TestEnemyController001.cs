using BattleSystem;
using CharacterSystem;
using UnityEngine;

public class TestEnemyController001 : TestEnemyController
{
    private float minFollowDistance = 0.1f;

    void Update()
    {
        if (BattleStage.now is null || BattleStage.now.mainCharacter is null)
            return;
        
        var pos = transform.position;
        Pawn target = BattleStage.now.mainCharacter;

        Vector2 toPlayer = (target.transform.position - pos);
        float dist = toPlayer.magnitude;
        if (dist > minFollowDistance)
        {
            owner.Move(toPlayer.normalized * scale);
        }
        else
        {
            owner.Move(Vector2.zero);
        }
    }
}
