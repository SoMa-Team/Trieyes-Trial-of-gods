using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Threading;
using BattleSystem;

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
        public float attackAngle = 90f; // 이거 절반으로 시계 방향, 시계 반대 방향으로 회전
        public float attackDuration = 1f;
        public float attackRadius = 1f; // 회전 반지름

        public Vector2 direction;
        public int segments = 8; // 부채꼴 세그먼트 수 (높을수록 부드러움)

        public override void Activate(Attack attack, Vector2 direction)
        {
            // 1. 캐릭터의 R_Weapon 게임 오브젝트를 가져옵니다. 여기가 공격 기준 좌표 입니다.
            var pawnPrefab = attack.attacker.pawnPrefab;
            var weaponGameObject = pawnPrefab.transform.Find("UnitRoot/Root/BodySet/P_Body/ArmSet/ArmR/P_RArm/P_Weapon/R_Weapon")?.gameObject;
            if (weaponGameObject == null)
            {
                Debug.LogError("R_Weapon을 찾지 못했습니다!");
                return;
            }

            attack.transform.SetParent(weaponGameObject.transform);
            attack.transform.localPosition = Vector3.zero;
            attack.transform.localRotation = Quaternion.Euler(0, 0, 0);

            attack.attackCollider = attack.gameObject.AddComponent<PolygonCollider2D>();
            var collider = attack.attackCollider as PolygonCollider2D;

            // 방향 벡터 → 각도 (라디안)
            direction = direction.normalized;
            Debug.Log($"direction: {direction}");

            // 부채꼴 모양의 콜라이더 포인트 생성
            Vector2[] points = CreateFanShapePoints(direction, attackAngle, attackRadius);
            collider.points = points;

            attack.attackCollider.isTrigger = true;
            attack.attackCollider.enabled = true;
        }

        public override void ProcessComponentCollision(Pawn targetPawn)
        {
            // AC1001_BUFF 버프 생성
            // 새로운 Attack 생성
            var buffAttack = AttackFactory.Instance.ClonePrefab(BUFF_ID);
            BattleStage.now.AttachAttack(buffAttack);
            buffAttack.target = targetPawn;
            var buffComponent = buffAttack.components[0] as AC1001_BUFF;
            buffComponent.buffType = BUFFType.IncreaseSpeed;
            buffComponent.buffValue = 10;
            buffComponent.buffDuration = 7f;
            buffAttack.Activate(attack.attacker, direction);

            buffAttack = AttackFactory.Instance.ClonePrefab(BUFF_ID);
            BattleStage.now.AttachAttack(buffAttack);
            buffAttack.target = targetPawn;
            buffComponent = buffAttack.components[0] as AC1001_BUFF;
            buffComponent.buffType = BUFFType.IncreaseAttackRangeAdd;
            buffComponent.buffValue = 3;
            buffComponent.buffDuration = 7f;
            buffAttack.Activate(attack.attacker, direction);

            buffAttack = AttackFactory.Instance.ClonePrefab(DEBUFF_ID);
            BattleStage.now.AttachAttack(buffAttack);
            buffAttack.target = targetPawn;
            var debuffComponent = buffAttack.components[0] as AC1000_DEBUFF;
            debuffComponent.debuffType = DEBUFFType.DecreaseDefense;
            debuffComponent.debuffValue = 10;
            debuffComponent.debuffDuration = 7f;
            buffAttack.Activate(attack.attacker, direction);
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

            //Debug.Log($"clockwiseDirection: {clockwiseDirection}, counterClockwiseDirection: {counterClockwiseDirection}");
            
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
            attack.transform.position = attack.attacker.transform.position;
            attack.transform.rotation = Quaternion.Euler(0, 0, 0);

            attackDuration -= Time.deltaTime;
            if (attackDuration <= 0f)
            {
                AttackFactory.Instance.Deactivate(attack);
            }
        }
    }
}