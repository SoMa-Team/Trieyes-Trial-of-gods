using UnityEngine;
using BattleSystem;

namespace CharacterSystem.Enemies
{
    /// <summary>
    /// 적 전용 컨트롤러. 플레이어를 추적해서 Pawn.Move로 이동시킴.
    /// </summary>
    [RequireComponent(typeof(Pawn))]
    public class EnemyController : Controller
    {
        public float minFollowDistance = 0.1f; // 너무 가까우면 멈춤
        private Transform playerTarget;

        private void Awake()
        {
        }

        private void Update()
        {
            if (owner == null || playerTarget == null || owner.isDead)
            {
                return;
            }

            Vector2 toPlayer = (playerTarget.position - transform.position);
            float dist = toPlayer.magnitude;
            if (dist > minFollowDistance)
            {
                owner.Move(toPlayer.normalized);
            }
            else
            {
                owner.Move(Vector2.zero); // 너무 가까우면 멈춤
            }
        }

        public override void Activate(Pawn pawn)
        {
            base.Activate(pawn);

            var playerObj = BattleStage.now.mainCharacter.gameObject;

            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }
    }
}