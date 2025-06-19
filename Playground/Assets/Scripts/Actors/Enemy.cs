using System.Collections;
using UnityEngine;

public class Enemy : Actor
{
    public Rigidbody2D target;
    public float knockbackSize = 3;
    public RuntimeAnimatorController[] controllers;

    private WaitForFixedUpdate wait = new WaitForFixedUpdate();

    private float lastDamageTime = -Mathf.Infinity;
    private float contactDamageCooldown = 0.5f;

    private void OnEnable()
    {
        isLive = true;
        currentHealth = statManager.vitalStats.maxHealth;

        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        animator.SetBool("Dead", false);
    }

    private void FixedUpdate()
    {
        if (!isLive || animator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
        {
            return;
        }

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * statManager.utilityStats.moveSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position+nextVec);
        rigid.linearVelocity = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (!isLive)
        {
            return;
        }

        spriter.flipX = target.position.x < rigid.position.x;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isLive) return;

        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null && Time.time - lastDamageTime > contactDamageCooldown)
            {
                lastDamageTime = Time.time;

                // Enemy가 Player에게 데미지를 줌
                player.TakeDamage(statManager.attakStats.attackDamage, statManager.attakStats.armorPenetration);
            }
        }
    }


    protected override void OnHit()
    {
        base.OnHit();
        StartCoroutine(KnockBack());
    }

    private IEnumerator KnockBack()
    {
        yield return wait;
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * knockbackSize * 3, ForceMode2D.Impulse);
    }

    protected override void Die()
    {
        rigid.simulated = false;
        spriter.sortingOrder = 1;
        GameManager.instance.killCount++;
        base.Die();
    }

    public void MakeUnable()
    {
        gameObject.SetActive(false);
    }
}
