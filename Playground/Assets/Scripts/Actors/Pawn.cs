using UnityEngine;
using Stats; // 새 StatSheet 네임스페이스

public class Pawn : MonoBehaviour
{
    [Header("Stats")]
    public StatSheet statSheet; // 새 구조 사용
    public float currentHealth;
    public bool isLive;

    protected Animator animator;
    protected Rigidbody2D rigid;
    protected Collider2D coll;
    protected SpriteRenderer spriter;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        spriter = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // StatSheet 인스턴스 연결 (Inspector에서 주입 or 여기서 생성)
        if (statSheet == null)
            statSheet = new StatSheet(); 
        currentHealth = statSheet[StatType.Health].Value;
        isLive = true;
    }

    public virtual void TakeDamage(float rawDamage, float armorPenetration)
    {
        // StatSheet에서 실시간 값 가져옴
        float defense = statSheet[StatType.Defense].Value;
        float effectiveDefense = Mathf.Max(0, defense * (100 - armorPenetration) / 100f);
        float damage = rawDamage * 100f / (100f + effectiveDefense);

        Debug.Log($"Damage: {damage}");
        Debug.Log($"before : {currentHealth}");
        currentHealth -= damage;
        Debug.Log($"after : {currentHealth}");

        // 체력 최소/최대값 체크 (statSheet의 MaxHealth로)
        float maxHealth = statSheet[StatType.Health].Value;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
            Die();
        else
            OnHit();
    }

    protected virtual void OnHit()
    {
        animator?.SetTrigger("Hit");
    }

    protected virtual void Die()
    {
        currentHealth = 0;
        coll.enabled = false;
        isLive = false;
        animator?.SetBool("Dead", true);
    }

    // 추가: 체력 회복, 스탯 변경 시 currentHealth 동기화 함수 등 필요 시 구현 가능
}