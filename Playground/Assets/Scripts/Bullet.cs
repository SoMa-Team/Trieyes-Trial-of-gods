using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rigid;

    private int leftPenetration;
    private float speed;
    private float range;
    private Vector2 startPos;

    private Actor owner;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 dir, int projectileCount, float projectileSpeed, float attackRange, Actor owner)
    {
        this.owner = owner;

        leftPenetration = projectileCount;
        speed = projectileSpeed;
        range = attackRange;

        startPos = rigid.position;
        rigid.linearVelocity = dir.normalized * speed;;
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
        Actor hitActor = collision.GetComponent<Actor>();

        if (hitActor == null || hitActor == owner)
            return;

        hitActor.TakeDamage(owner.statManager.attakStats.attackDamage, owner.statManager.attakStats.armorPenetration);
        
        leftPenetration--;

        if (leftPenetration <= 0)
        {
            rigid.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }
}
