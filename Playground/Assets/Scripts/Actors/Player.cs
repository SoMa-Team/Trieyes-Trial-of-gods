using UnityEngine;
using Stats;

namespace Actors
{
    public class Player : Pawn
    {
        public Vector2 inputVec;

        private Vector3 fixedDeathPosition;
        void Update()
        {
            if (!isLive) return;
            // 키보드 입력으로 방향벡터 계산
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            inputVec = new Vector2(h, v).normalized;
        }

        void FixedUpdate()
        {
            if (!isLive) return;
            float moveSpeed = statSheet[StatType.MoveSpeed].Value; // StatSheet에서 이동속도 읽기
            Vector2 nextVec = inputVec * moveSpeed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
        }

        private void LateUpdate()
        {
            if (!isLive)
            {
                transform.position = fixedDeathPosition;
                return;
            }
            animator.SetFloat("Speed", inputVec.magnitude); 
            if (inputVec.x != 0)
            {
                spriter.flipX = inputVec.x < 0;
            }
        }

        protected override void Die()
        {
            fixedDeathPosition = transform.position;
            base.Die();
        }
    }
}