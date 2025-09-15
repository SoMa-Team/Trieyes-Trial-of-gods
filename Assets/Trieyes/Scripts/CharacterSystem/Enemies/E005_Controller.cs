using System;
using UnityEngine;
using CharacterSystem;
using BattleSystem;
using Stats;

namespace Enemies
{
    /// <summary>
    /// 적 전용 컨트롤러. 플레이어를 추적해서 Pawn.Move로 이동시킴.
    /// </summary>
    [RequireComponent(typeof(Pawn))]
    public class E005_Controller : EnemyController
    {
        private Transform playerTarget;

        private E005_BlueGolem enemy;

        private Vector3 targetCollisionOffset;

        public override void Update()
        {
            if (owner == null || playerTarget == null || enemy.isDead)
            {
                return;
            }

            base.Update();
        }

        public override void Activate(Pawn pawn)
        {
            base.Activate(pawn);

            var playerObj = BattleStage.now.mainCharacter.gameObject;
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
                targetCollisionOffset = BattleStage.now.mainCharacter.CenterOffset;
            }
            
            enemy = pawn as E005_BlueGolem;
        }
    }
}