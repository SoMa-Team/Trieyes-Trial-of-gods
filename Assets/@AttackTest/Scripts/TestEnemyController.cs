using BattleSystem;
using CharacterSystem;
using UnityEngine;

public class TestEnemyController : MonoBehaviour
{
    public Pawn owner;
    public float scale = 1;
    
    void Start()
    {
                
    }

    void Update()
    {
        if (BattleStage.now is null || BattleStage.now.mainCharacter is null)
            return;
        
        var pos = transform.position;
        var targetPos = BattleStage.now.mainCharacter.transform.position;
        var dir = (targetPos - pos).normalized * scale;
        owner.Move(new Vector2(dir.x, dir.y));
    }
}
