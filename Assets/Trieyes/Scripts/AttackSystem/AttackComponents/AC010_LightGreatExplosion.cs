using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Collections.Generic;
using BattleSystem;
using CharacterSystem.Enemies;

namespace AttackComponents
{
    /// <summary>
    /// 빛의 대폭발 효과
    /// 기본 공격이 맞은 대상 주위에 자그마한 원형 폭발을 일으킵니다.
    /// AC002의 로직을 기반으로 하여 각 히트된 적에게 AC100을 소환합니다.
    /// </summary>
    public class AC010_LightGreatExplosion : AttackComponent
    {
        [Header("빛의 대폭발 설정")]
        public float explosionDamage = 80f;
        public float explosionRadius = 4f;
        public float explosionDuration = 1.5f;
        public float explosionDelay = 0.3f;
        public float chargeTime = 0.5f; // 충전 시간

        // AC002 로직 복사
        [Header("AC002 공격 설정")]
        public float attackAngle = 90f; // 이거 절반으로 시계 방향, 시계 반대 방향으로 회전
        public float attackDuration = 1f;
        public float attackRadius = 1f; // 회전 반지름
        public int segments = 8; // 부채꼴 세그먼트 수 (높을수록 부드러움)

        [Header("VFX 설정")]
        public GameObject explosionVFXPrefab;
        public float vfxDuration = 0.6f;

        // AC100 소환 설정
        [Header("AC100 소환 설정")]
        private const int AC100_ID = 10; // AC100의 ID

        // 폭발 상태 관리
        private ExplosionState explosionState = ExplosionState.None;
        private float explosionTimer = 0f;
        private float chargeTimer = 0f;
        private Vector2 targetPosition;
        private List<Enemy> hitTargets = new List<Enemy>(15);

        // AC002 로직 관련
        private GameObject weaponGameObject; // 무기 오브젝트 참조
        private BoxCollider2D attackCollider;

        // 폭발 상태 열거형
        private enum ExplosionState
        {
            None,
            Preparing,
            Exploding,
            Impact,
            Finished
        }

        // 재사용 가능한 콜라이더 리스트 (GC 최적화)
        private List<Collider2D> reusableColliders = new List<Collider2D>(30);

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // AC002 로직 초기화
            InitializeAC002Logic();
            
            // 초기 상태 설정
            explosionState = ExplosionState.None;
            explosionTimer = 0f;
            chargeTimer = 0f;
            hitTargets.Clear();
            
            // 타겟 위치 설정
            targetPosition = (Vector2)attacker.transform.position + direction * 5f;
            
            // 폭발 시작
            StartExplosion();
        }

        private void InitializeAC002Logic()
        {
            // 1. R_Weapon 오브젝트 찾기
            var pawnPrefab = attack.attacker.pawnPrefab;
            weaponGameObject = pawnPrefab.transform.Find("UnitRoot/Root/BodySet/P_Body/ArmSet/ArmR/P_RArm/P_Weapon/R_Weapon")?.gameObject;

            if (weaponGameObject == null)
            {
                Debug.LogError("R_Weapon을 찾지 못했습니다!");
                return;
            }

            // 2. attack 오브젝트를 R_Weapon의 자식으로 설정
            attack.transform.SetParent(weaponGameObject.transform);
            attack.transform.localPosition = Vector3.zero;
            attack.transform.localRotation = Quaternion.identity;

            // 3. attack 오브젝트에 콜라이더 추가 및 설정
            attackCollider = attack.GetComponent<BoxCollider2D>();
            if (attackCollider == null)
            {
                attackCollider = attack.gameObject.AddComponent<BoxCollider2D>();
            }
            attackCollider.offset = new Vector2(0, 0.3f);
            attackCollider.size = new Vector2(0.16f, 0.56f);
            attackCollider.isTrigger = true;
            attackCollider.enabled = true;
            attack.attackCollider = attackCollider;

            // 4. 애니메이션 트리거
            attack.attacker.ChangeAnimationState("ATTACK");
        }

        public override void ProcessComponentCollision(Pawn targetPawn)
        {
            // AC100 소환
            var aoeAttack = AttackFactory.Instance.ClonePrefab(AC100_ID);
            BattleStage.now.AttachAttack(aoeAttack);
            aoeAttack.target = targetPawn;
            
            // AC100 설정
            var aoeComponent = aoeAttack.components[0] as AC100_AOE;
            if (aoeComponent != null)
            {
                aoeComponent.aoeMode = AOEMode.SingleHit;
                aoeComponent.createAreaAttack = true;
                aoeComponent.areaAttackRadius = 1.5f;
                aoeComponent.areaAttackDamage = explosionDamage;
                aoeComponent.areaAttackVFXDuration = 0.3f;
            }
            
            aoeAttack.Activate(attack.attacker, Vector2.zero);
            
            Debug.Log($"<color=cyan>[AC010] {targetPawn.pawnName}에게 AC100 AOE 소환</color>");
        }

        private void StartExplosion()
        {
        }

        protected override void Update()
        {
            base.Update();
            
            // AC002 위치 업데이트
            if (weaponGameObject != null)
            {
                attack.transform.position = attack.attacker.transform.position;
            }
        
            attackDuration -= Time.deltaTime;
            if (attackDuration <= 0f)
            {
                AttackFactory.Instance.Deactivate(attack);
            }
        }

        private void CreatePreparationVFX()
        {
            // 준비 VFX 생성 로직
        }

        private void CreateExplosionVFX()
        {
            // 폭발 VFX 생성 로직
        }

        private void CreateImpactVFX()
        {
            // 충격 VFX 생성 로직
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
            // AC002 정리 로직
            if (weaponGameObject != null)
            {
                foreach (var collider in weaponGameObject.GetComponents<BoxCollider2D>())
                {
                    Destroy(collider);
                }
            }
        }
    }
} 