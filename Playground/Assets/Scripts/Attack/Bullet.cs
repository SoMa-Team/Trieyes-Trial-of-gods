using UnityEngine;
using Stats;
using Actors;

namespace Attack
{
    public class Bullet : MonoBehaviour
    {
        private Rigidbody2D rigid;

        private int leftPenetration;
        private float speed;
        private float range;
        private Vector2 startPos;

        private Pawn owner;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody2D>();
        }

        public void Init(Vector2 dir, int projectileCount, float projectileSpeed, float attackRange, Pawn owner)
        {
            this.owner = owner;

            leftPenetration = projectileCount;
            speed = projectileSpeed;
            range = attackRange;

            startPos = rigid.position;
            rigid.linearVelocity = dir.normalized * speed;
        }

        private void FixedUpdate()
        {
            if (Vector2.Distance(startPos, rigid.position) > range)
            {
                rigid.linearVelocity = Vector2.zero;
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Pawn hitPawn = collision.GetComponent<Pawn>();

            if (hitPawn == null || hitPawn == owner)
                return;

            // StatSheet 기반으로 공격력/방어관통 읽기
            float attackDamage = owner.statSheet[StatType.AttackPower].Value;
            float armorPenetration = owner.statSheet[StatType.DefensePenetration].Value;

            hitPawn.TakeDamage(attackDamage, armorPenetration);

            leftPenetration--;

            if (leftPenetration <= 0)
            {
                rigid.linearVelocity = Vector2.zero;
                gameObject.SetActive(false);
            }
        }
    }   
}