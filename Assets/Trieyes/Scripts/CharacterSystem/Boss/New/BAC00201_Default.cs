using AttackComponents;
using AttackSystem;
using CharacterSystem;
using RelicSystem;
using UnityEngine;

public class BAC00201_Default : AttackComponent
{
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private ParticleSystem particle;

    [Header("======= 유물 스탯 적용 부분 =======")]
    private int projectileCount => attack.getRelicStat(RelicStatType.ProjectileCount); // 투사체 개수 (근딜 의미 X)
    private float skillDistance => attack.getRelicStat(RelicStatType.Range); // 스킬 사거리 (원딜 의미 낮음)
    private float aoeSize => attack.getRelicStat(RelicStatType.AOE); // AOE 크기(공격 범위의 크기)
    private float pierceCount => attack.getRelicStat(RelicStatType.ProjectilePierce); // 관통 개수 (근딜 의미 X)
    
    // 공격 Default 상수
    private float projectileSpeed = 6;
    private float totalMaxAngle = 120;
    private float defaultAngle = 30;
    private float defaultSkillDuration = 1.2f;
    
    private float startTime;
    private float remainPierce;
    
    public override void Activate(Attack attack, Vector2 direction)
    {
        base.Activate(attack, direction);
        trail.enabled = true;
        particle.Play();
        startTime = Time.time; 
        
        attack.transform.localScale = aoeSize * Vector3.one;
        remainPierce = pierceCount;
        
        ApplyProjectileCount();
    }

    public override void Deactivate()
    {
        base.Deactivate();
        trail.Clear();
        trail.enabled = false;
        particle.Stop();
    }

    protected override void Update()
    {
        base.Update();

        if (Time.time - startTime > defaultSkillDuration * skillDistance)
        {
            AttackFactory.Instance.Deactivate(attack);
            return;
        }
        
        attack.transform.position += attack.transform.right * (projectileSpeed * Time.deltaTime);
    }

    public override void ProcessComponentCollision(Pawn targetPawn)
    {
        if (--remainPierce <= 0)
        {
            AttackFactory.Instance.Deactivate(attack);
        }
    }

    private void ApplyProjectileCount()
    {
        // 메인 Attack이 아닐 경우 return
        if (attack.parent is not null)
            return;

        // 자신을 제외한 나머지 공격 생성 및 위치 조정
        for (int i = 1; i < projectileCount; i++)
        {
            var childAttack = AttackFactory.Instance.Create(attack.attackData, attack.attacker, attack, attack.transform.right);
            
            foreach (var component in childAttack.components)
            {
                var attackComponent = component as BAC00201_Default;
                
                if (attackComponent is null)
                    continue;
                
                attackComponent.SetRotate(i, projectileCount);
            }
        }
        
        // 자신도 올바른 위치로 수정
        SetRotate(0, projectileCount);
    }

    private void SetRotate(int index, int total)
    {
        if (total == 1)
            return;
        
        // 각 공격은 defaultAngle 각도로 벌어짐
        var offset = index * defaultAngle - (total - 1) * defaultAngle / 2;
        
        // 만약 모든 공격이 totalMaxAngle이상 벌어진다면, totalMaxAngle 이하로 벌어지도록 수정
        if (total * defaultAngle > totalMaxAngle)
            offset = index * totalMaxAngle / (total - 1) - totalMaxAngle / 2;
        
        attack.transform.Rotate(0, 0, offset);
    }
}