using System.Collections;
using UnityEngine;
using Stats;

public class Enemy : Pawn
{
    public Rigidbody2D target;
    public float knockbackSize = 3;
    public RuntimeAnimatorController[] controllers;

    private WaitForFixedUpdate wait = new WaitForFixedUpdate();

    private float lastDamageTime = -Mathf.Infinity;
    private float contactDamageCooldown = 0.5f;
    
    public static class EnemyStatPreset
    {
        public const int Health = 100;
        public const int AttackPower = 20;
        public const int MoveSpeed = 5;
        public const int ProjectileCount = 1;
        public const int ProjectilePierce = 0;
        public const int AttackSpeed = 1;
        public const int AttackRange = 8;
        public const int Defense = 3;
        public const int DefensePenetration = 0;
    }

    protected void Start()
    {
        InitializeStats();
    }

    private void InitializeStats()
    {
        if (statSheet == null)
            statSheet = new StatSheet();

        statSheet[StatType.Health].SetBasicValue(EnemyStatPreset.Health);
        statSheet[StatType.AttackPower].SetBasicValue(EnemyStatPreset.AttackPower);
        statSheet[StatType.MoveSpeed].SetBasicValue(EnemyStatPreset.MoveSpeed);
        statSheet[StatType.ProjectileCount].SetBasicValue(EnemyStatPreset.ProjectileCount);
        statSheet[StatType.ProjectilePierce].SetBasicValue(EnemyStatPreset.ProjectilePierce);
        statSheet[StatType.AttackSpeed].SetBasicValue(EnemyStatPreset.AttackSpeed);
        statSheet[StatType.AttackRange].SetBasicValue(EnemyStatPreset.AttackRange);
        statSheet[StatType.Defense].SetBasicValue(EnemyStatPreset.Defense);
        statSheet[StatType.DefensePenetration].SetBasicValue(EnemyStatPreset.DefensePenetration);

        currentHealth = statSheet[StatType.Health].Value;
        isLive = true;
    }

    private void OnEnable()
    {
        isLive = true;
        // StatSheet 기반으로 초기화
        currentHealth = statSheet[StatType.Health].Value;

        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        animator.SetBool("Dead", false);
    }

    private void FixedUpdate()
    {
        if (!isLive || animator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        Vector2 dirVec = target.position - rigid.position;
        float moveSpeed = statSheet[StatType.MoveSpeed].Value;
        Vector2 nextVec = dirVec.normalized * moveSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.linearVelocity = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (!isLive)
            return;

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

                // StatSheet에서 공격력과 방어관통력 가져오기
                float attackDamage = statSheet[StatType.AttackPower].Value;
                float armorPenetration = statSheet[StatType.DefensePenetration].Value;
                player.TakeDamage(attackDamage, armorPenetration);
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
