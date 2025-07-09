using UnityEngine;
using BattleSystem;

namespace CharacterSystem.Enemies
{
    /// <summary>
    /// 적 전용 컨트롤러. 플레이어를 추적해서 Pawn.Move로 이동시킴.
    /// </summary>
    [RequireComponent(typeof(Pawn))]
    public class EnemyController : MonoBehaviour
    {
        public float minFollowDistance = 0.1f; // 너무 가까우면 멈춤
        private Pawn pawn;
        private Transform playerTarget;

        private void Awake()
        {
            pawn = GetComponent<Pawn>();

            var playerObj = BattleStage.now.mainCharacter.gameObject;

            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }

        private void Update()
        {
            if (pawn == null || playerTarget == null || pawn.isDead)
            {
                return;
            }

            // StatSheet에서 최신 MoveSpeed를 반영
            pawn.moveSpeed = pawn.GetStatValue(Stats.StatType.MoveSpeed);

            Vector2 toPlayer = (playerTarget.position - transform.position);
            float dist = toPlayer.magnitude;
            if (dist > minFollowDistance)
            {
                pawn.Move(toPlayer.normalized);
            }
            else
            {
                pawn.Move(Vector2.zero); // 너무 가까우면 멈춤
            }
        }
    }
} 