using System;
using BattleSystem;
using CharacterSystem;
using UnityEngine;

public class TestKeyboardController : MonoBehaviour
{
    private bool isKeyboardMove = false;

    private void Start()
    {
    }

    private void Update()
    {
        if (BattleStage.now is null)
            return;

        if (isKeyboardMove)
        {
            var direction = new Vector2(0, 0);

            if (Input.GetKey(KeyCode.W))
                direction.y++;
            if (Input.GetKey(KeyCode.A))
                direction.x--;
            if (Input.GetKey(KeyCode.S))
                direction.y--;
            if (Input.GetKey(KeyCode.D))
                direction.x++;
        
            BattleStage.now.mainCharacter.Controller.moveDir = direction;
            BattleStage.now.mainCharacter.Move(direction);
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        {
            isKeyboardMove = !isKeyboardMove;
            ((PlayerController)BattleStage.now.mainCharacter.Controller).SetJoystickActivate(!isKeyboardMove);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            BattleStage.now.mainCharacter.ExecuteAttack(PawnAttackType.BasicAttack);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            BattleStage.now.mainCharacter.ExecuteAttack(PawnAttackType.Skill1);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            BattleStage.now.mainCharacter.ExecuteAttack(PawnAttackType.Skill2);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            BattleStage.now.mainCharacter.isAutoAttack = !BattleStage.now.mainCharacter.isAutoAttack;
        }
    }
}
