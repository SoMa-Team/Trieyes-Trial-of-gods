using System;
using CharacterSystem.Enemies;
using UnityEngine;

namespace CharacterSystem
{
    public class B002_Controller : Controller
    {
        private B002_Water boss;
        
        public override void Activate(Pawn pawn)
        {
            base.Activate(pawn);
            boss = pawn as B002_Water;
        }

        // TODO : Test용 플레이어블 보스
        private void Update()
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
            
            boss.Move(direction.normalized);
            moveDir = direction;
            
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                boss.ExecuteBossAttack(B002AttackType.Default);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                boss.ExecuteBossAttack(B002AttackType.StoneSummon);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                boss.ExecuteBossAttack(B002AttackType.Rush);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                boss.ExecuteBossAttack(B002AttackType.StoneExplode);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                boss.ExecuteBossAttack(B002AttackType.FireDischarge);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                boss.ExecuteBossAttack(B002AttackType.SpawnSlowField);
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                boss.ExecuteBossAttack(B002AttackType.CircularSector);
                return;
            }
        }
    }
}