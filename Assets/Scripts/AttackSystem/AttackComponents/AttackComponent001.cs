using UnityEngine;
using AttackSystem;
using CharacterSystem;
using Utils;
using System.Linq;

namespace AttackComponents
{
    public class AttackComponent001 : AttackComponent
    {
        public float throwForce = 10f;    // 던지는 힘
        private Vector2 throwDir;
        
        protected override void Start()
        {
            base.Start();
            prefab = Resources.Load<GameObject>("@/CharacterTest/Prefabs/Attack/Attack001Prefab");
        }

        protected override void Update()
        {
            base.Update();
            if (rb != null)
            {
                rb.linearVelocity = throwDir * throwForce;
            }
        }

        // 이 함수는 Attack 트리거 시 호출된다고 가정
        public override void Execute(Attack attack)
        {
            parentAttack = attack;
            owner = parentAttack.attacker;

            // Attack(GameObject)의 projectiles 리스트에 자신을 추가 (SetParent는 하지 않음)
            if (parentAttack != null)
            {
                if (parentAttack.projectiles != null && !parentAttack.projectiles.Contains(gameObject))
                {
                    parentAttack.projectiles.Add(gameObject);
                }
            }

            // 기존 세팅
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            throwDir = owner.IsFacingRight() ? owner.transform.right : -owner.transform.right;
            if (spriteRenderer != null)
                spriteRenderer.flipX = throwDir.x >= 0;
            if (rb != null)
                rb.linearVelocity = throwDir * throwForce;
        }
    }
}