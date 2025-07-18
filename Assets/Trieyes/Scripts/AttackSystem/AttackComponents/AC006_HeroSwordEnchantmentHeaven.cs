using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Threading;
using BattleSystem;
using System.Collections.Generic;

namespace AttackComponents
{
    /// <summary>
    /// 캐릭터 소드 능력 부여 강화
    /// 캐릭터 소드 공격은 캐릭터 소드 공격 로직을 만듭니다.
    /// 7초 동안 검에 무작위 속성을 부여하고, 기본 공격(AC002)에 다음의 추가효과가 적용되고, 추가 피해를 입힙니다.
    /// 천상 : 이동속도와 사거리가 증가합니다. 방어력이 감소합니다. AC1001_BUFF 버프를 줍니다.
    /// </summary>
    public class AC006_HeroSwordEnchantmentHeaven : AttackComponent
    {
        private const int BUFF_ID = 13;
        private const int DEBUFF_ID = 11;
        private const int AC100_ID = 10;
        public float attackAngle = 90f; // 이거 절반으로 시계 방향, 시계 반대 방향으로 회전
        public float attackDuration = 1f;
        public float attackRadius = 1f; // 회전 반지름

        public Vector2 direction;
        public int segments = 8; // 부채꼴 세그먼트 수 (높을수록 부드러움)

        // FSM 상태 관리
        private HeavenAttackState attackState = HeavenAttackState.None;
        private float attackTimer = 0f;
        private Vector2 attackDirection;

        // 천상 공격 상태 열거형
        private enum HeavenAttackState
        {
            None,
            Preparing,
            Active,
            Finishing,
            Finished
        }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            attackState = HeavenAttackState.None;
            attackTimer = 0f;
            attackDirection = direction.normalized;
            
            // 천상 공격 시작
            StartHeavenAttack();
        }

        private void StartHeavenAttack()
        {
            attackState = HeavenAttackState.Preparing;
            attackTimer = 0f;
            
            // 1. 캐릭터의 R_Weapon 게임 오브젝트를 가져옵니다. 여기가 공격 기준 좌표 입니다.
            var pawnPrefab = attack.attacker.pawnPrefab;
            var weaponGameObject = pawnPrefab.transform.Find("UnitRoot/Root/BodySet/P_Body/ArmSet/ArmR/P_RArm/P_Weapon/R_Weapon")?.gameObject;
            if (weaponGameObject == null)
            {
                //debug.logError("R_Weapon을 찾지 못했습니다!");
                return;
            }

            attack.transform.SetParent(weaponGameObject.transform);
            attack.transform.localPosition = Vector3.zero;
            attack.transform.localRotation = Quaternion.Euler(0, 0, 0);

            attack.attackCollider = attack.gameObject.AddComponent<PolygonCollider2D>();
            var collider = attack.attackCollider as PolygonCollider2D;

            // 부채꼴 모양의 콜라이더 포인트 생성
            Vector2[] points = CreateFanShapePoints(attackDirection, attackAngle, attackRadius);
            collider.points = points;

            attack.attackCollider.isTrigger = true;
            attack.attackCollider.enabled = true;
            
            //debug.log("<color=white>[AC006] 천상 강화 공격 시작!</color>");
        }

        public override void ProcessComponentCollision(Pawn targetPawn)
        {
            // 새로운 BUFF 클래스 사용 - Haste 효과 (이동속도 + 공격속도 증가)
            var hasteBuffInfo = new BuffInfo
            {
                buffType = BUFFType.Haste,
                attack = attack,
                targets = new List<Pawn> { attack.attacker }, // 자신에게 버프
                buffValue = 10,
                buffMultiplier = 1.5f,
                buffDuration = 7f,
                buffInterval = 1f,
                globalHeal = 0
            };

            var hasteBuff = new BUFF();
            hasteBuff.Activate(hasteBuffInfo);

            // 새로운 BUFF 클래스 사용 - 공격범위 증가
            var rangeBuffInfo = new BuffInfo
            {
                buffType = BUFFType.IncreaseAttackRangeAdd,
                attack = attack,
                targets = new List<Pawn> { attack.attacker }, // 자신에게 버프
                buffValue = 3,
                buffMultiplier = 1f,
                buffDuration = 7f,
                buffInterval = 1f,
                globalHeal = 0
            };

            var rangeBuff = new BUFF();
            rangeBuff.Activate(rangeBuffInfo);

            // 새로운 DEBUFF 클래스 사용 - 방어력 감소
            var debuffInfo = new DebuffInfo
            {
                debuffType = DEBUFFType.DecreaseDefense,
                attack = attack,
                target = attack.attacker, // 자신에게 디버프
                debuffValue = 10,
                debuffMultiplier = 0.5f,
                debuffDuration = 7f,
                debuffInterval = 1f,
                globalDamage = 0
            };

            var debuff = new DEBUFF();
            debuff.Activate(debuffInfo);

            // 빛 속성일 때 AOE 공격
            var hero = attack.attacker as Character001_Hero;
            if (hero.weaponElementState == HeroWeaponElementState.Light)
            {
                var aoeAttack = AttackFactory.Instance.ClonePrefab(AC100_ID);
                BattleStage.now.AttachAttack(aoeAttack);
                aoeAttack.target = targetPawn;
                
                aoeAttack.Activate(attack.attacker, Vector2.zero);
            }
        }

        /// <summary>
        /// 방향 벡터를 기준으로 부채꼴 모양의 콜라이더 포인트를 생성합니다.
        /// </summary>
        /// <param name="direction">기준 방향 벡터</param>
        /// <param name="totalAngle">전체 각도 (이 값의 절반씩 양쪽으로 회전)</param>
        /// <param name="radius">부채꼴 반지름</param>
        /// <returns>PolygonCollider2D에 사용할 포인트 배열</returns>
        private Vector2[] CreateFanShapePoints(Vector2 direction, float totalAngle, float radius)
        {
            // 중심점 + 호를 따라 생성되는 점들
            Vector2[] points = new Vector2[segments + 2];
            
            // 첫 번째 점은 중심점 (0, 0)
            points[0] = Vector2.zero;
            
            // 절반 각도로 시계 방향과 시계 반대 방향 계산
            float halfAngle = totalAngle * 0.5f;
            
            // 시계 방향과 시계 반대 방향 벡터 계산
            Vector2 clockwiseDirection = RotateVector2D(direction, -halfAngle);
            Vector2 counterClockwiseDirection = RotateVector2D(direction, halfAngle);

            ////debug.log($"clockwiseDirection: {clockwiseDirection}, counterClockwiseDirection: {counterClockwiseDirection}");
            
            // 부채꼴 호를 따라 점들 생성
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments; // 0부터 1까지
                
                // 시계 방향에서 시계 반대 방향으로 보간
                Vector2 currentDirection = Vector2.Lerp(clockwiseDirection, counterClockwiseDirection, t).normalized;
                points[i + 1] = currentDirection * radius;
            }
            
            return points;
        }

        /// <summary>
        /// 2D 벡터를 주어진 각도만큼 회전시킵니다.
        /// </summary>
        /// <param name="vector">회전시킬 벡터</param>
        /// <param name="degrees">회전 각도 (도 단위)</param>
        /// <returns>회전된 벡터</returns>
        private Vector2 RotateVector2D(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            
            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }

        protected override void Update()
        {
            base.Update();
            
            // 천상 공격 상태 처리
            ProcessHeavenAttackState();
        }

        private void ProcessHeavenAttackState()
        {
            switch (attackState)
            {
                case HeavenAttackState.None:
                    break;

                case HeavenAttackState.Preparing:
                    attackTimer += Time.deltaTime;
                    
                    if (attackTimer >= 0.1f) // 준비 시간
                    {
                        attackState = HeavenAttackState.Active;
                        attackTimer = 0f;
                        ActivateHeavenAttack();
                    }
                    break;

                case HeavenAttackState.Active:
                    attackTimer += Time.deltaTime;
                    
                    // 위치 업데이트
                    attack.transform.position = attack.attacker.transform.position;
                    attack.transform.rotation = Quaternion.Euler(0, 0, 0);
                    
                    if (attackTimer >= attackDuration)
                    {
                        attackState = HeavenAttackState.Finishing;
                        attackTimer = 0f;
                        FinishHeavenAttack();
                    }
                    break;

                case HeavenAttackState.Finishing:
                    attackTimer += Time.deltaTime;
                    
                    if (attackTimer >= 0.1f) // 종료 시간
                    {
                        attackState = HeavenAttackState.Finished;
                    }
                    break;

                case HeavenAttackState.Finished:
                    attackState = HeavenAttackState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ActivateHeavenAttack()
        {
            //debug.log("<color=green>[AC006] 천상 강화 공격 활성화!</color>");
        }

        private void FinishHeavenAttack()
        {
            //debug.log("<color=white>[AC006] 천상 강화 공격 종료!</color>");
        }
    }
}