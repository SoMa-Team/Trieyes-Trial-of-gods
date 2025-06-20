using System;
using UnityEngine;
using Stats; // StatSheet 네임스페이스

public class Player : Pawn
{
    public Vector2 inputVec;

    private Vector3 fixedDeathPosition;
    
    public static class PlayerStatPreset
    {
        public const int Health = 100000;
        public const int AttackPower = 35;
        public const int MoveSpeed = 8;
        public const int ProjectileCount = 1;
        public const int ProjectilePierce = 20;
        public const int AttackSpeed = 10;
        public const int AttackRange = 15;
        public const int Defense = 1000;
        public const int DefensePenetration = 10;
    }//스탯 초기값
    
    protected void Start()
    {
        Debug.Log("Player Start 호출");
        InitializeStats();
    }

    private void InitializeStats()// 스탯 초기화 함수
    {
        Debug.Log("Player InitializeStats 호출"); 
        if (statSheet == null)
            statSheet = new StatSheet();

        statSheet[StatType.Health].SetBasicValue(PlayerStatPreset.Health);
        statSheet[StatType.AttackPower].SetBasicValue(PlayerStatPreset.AttackPower);
        statSheet[StatType.MoveSpeed].SetBasicValue(PlayerStatPreset.MoveSpeed);
        statSheet[StatType.ProjectileCount].SetBasicValue(PlayerStatPreset.ProjectileCount);
        statSheet[StatType.ProjectilePierce].SetBasicValue(PlayerStatPreset.ProjectilePierce);
        statSheet[StatType.AttackSpeed].SetBasicValue(PlayerStatPreset.AttackSpeed);
        statSheet[StatType.AttackRange].SetBasicValue(PlayerStatPreset.AttackRange);
        statSheet[StatType.Defense].SetBasicValue(PlayerStatPreset.Defense);
        statSheet[StatType.DefensePenetration].SetBasicValue(PlayerStatPreset.DefensePenetration);

        currentHealth = statSheet[StatType.Health].Value;
        isLive = true;
    }


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