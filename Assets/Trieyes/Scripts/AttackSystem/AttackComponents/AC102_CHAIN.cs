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
    /// 번개 연쇄 효과
    /// 지정된 횟수만큼 주변 적을 찾아가면서 번개 데미지를 입힙니다.
    /// GC 최적화를 위해 코루틴 대신 재귀적 처리를 사용합니다.
    /// </summary>
    public class AC102_CHAIN : AttackComponent
    {
        [Header("번개 연쇄 설정")]
        public int chainDamage;
        public float chainRadius; // 번개가 전염되는 최대 거리
        public int chainCount; // 번개 전염 횟수
        public float chainDelay; // 연쇄 간격

        [Header("VFX 설정")]
        [SerializeField] public GameObject chainVFXPrefab; // 번개 연쇄 VFX 프리팹 (외부에서 설정 가능)
        public float lightningDuration = 0.2f;
        private GameObject spawnedVFX;

        // 번개 연쇄 상태 관리
        private LightningChainState chainState = LightningChainState.None;
        private float chainTimer = 0f;

        public List<Enemy> targetEnemies = new List<Enemy>();
        private List<Vector2> chainPositions = new List<Vector2>();
        private int currentChainCount = 0;
        private Vector2 currentChainPosition;

        // 번개 연쇄 상태 열거형
        private enum LightningChainState
        {
            None,
            Starting,
            Chaining,
            Finished
        }

        // 타겟 관리용 큐 (거리 순으로 정렬된 Pawn들)
        private Queue<Pawn> targetQueue = new Queue<Pawn>();
        private Vector2 initialPosition; // 초기 시작 위치

        // 클래스 멤버로 선언
        private List<Collider2D> reusableColliders = new List<Collider2D>(20);

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            // 초기 상태 설정
            chainState = LightningChainState.None;
            chainTimer = 0f;
            chainPositions.Clear();
            currentChainCount = 0;
        }

        public void StartLightningChain(Vector2 startPosition)
        {
            // 초기 상태 설정
            chainState = LightningChainState.Starting;
            chainTimer = 0f;
            chainPositions.Clear();
            chainPositions.Add(startPosition);
            currentChainCount = 0;
            currentChainPosition = startPosition;
            initialPosition = startPosition;

            // 초기 범위에서 모든 적을 찾아서 거리 순으로 정렬
            InitializeTargetQueue(startPosition);

            // 첫 번째 타겟에 데미지 적용
            ApplyLightningDamage();
        }

        protected override void Update()
        {
            base.Update();

            // 번개 연쇄 처리
            ProcessLightningChain();
        }

        private void ProcessLightningChain()
        {
            switch (chainState)
            {
                case LightningChainState.None:
                    // 아무것도 하지 않음
                    break;

                case LightningChainState.Starting:
                    // 연쇄 시작
                    chainState = LightningChainState.Chaining;
                    chainTimer = 0f;
                    break;

                case LightningChainState.Chaining:
                    chainTimer += Time.deltaTime;

                    if (chainTimer >= chainDelay)
                    {
                        ProcessNextChain();
                        chainTimer = 0f;
                    }
                    break;

                case LightningChainState.Finished:
                    chainState = LightningChainState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ProcessNextChain()
        {
            // 현재 체인 카운트 증가
            currentChainCount++;

            // 체인 카운트가 최대치에 도달했거나 큐가 비어있으면 종료
            if (currentChainCount >= chainCount || targetQueue.Count == 0)
            {
                chainState = LightningChainState.Finished;
                return;
            }

            // 다음 타겟 처리
            ApplyLightningDamage();
        }

        private void InitializeTargetQueue(Vector2 startPosition)
        {
            // 기존 큐 클리어
            targetQueue.Clear();

            // 범위 내 모든 적 찾기
            Collider2D[] colliders = Physics2D.OverlapCircleAll(startPosition, chainRadius);
            reusableColliders.Clear();
            reusableColliders.AddRange(colliders);

            // 적들만 필터링하고 거리 순으로 정렬
            List<Pawn> enemiesInRange = new List<Pawn>();

            foreach (Collider2D collider in reusableColliders)
            {
                Pawn pawn = collider.GetComponent<Pawn>();
                if (pawn is Enemy enemy && enemy != attack.attacker)
                {
                    float distance = Vector2.Distance(startPosition, enemy.transform.position);
                    enemiesInRange.Add(enemy);
                }
            }

            // 거리 순으로 정렬
            enemiesInRange.Sort((a, b) =>
            {
                float distanceA = Vector2.Distance(startPosition, a.transform.position);
                float distanceB = Vector2.Distance(startPosition, b.transform.position);
                return distanceA.CompareTo(distanceB);
            });

            // 큐에 추가
            foreach (Pawn enemy in enemiesInRange)
            {
                targetQueue.Enqueue(enemy);
                Debug.Log($"targetQueue: {enemy.pawnName}, Position: {enemy.transform.position}");
            }
        }

        private void CreateLightningVFX(Vector2 start, Vector2 end)
        {
            // VFX 프리팹이 없으면 VFX 없이 진행
            if (chainVFXPrefab == null)
            {
                return;
            }

            // AC001 방식으로 VFX 생성
            Vector2 vfxPosition = (start + end) * 0.5f; // 중간 위치
            spawnedVFX = CreateAndSetupVFX(chainVFXPrefab, vfxPosition, Vector2.zero);
            
            // LightningVFX 컴포넌트 찾아서 번개 효과 시작
            LightningVFX lightningVFX = spawnedVFX.GetComponent<LightningVFX>();
            if (lightningVFX != null)
            {
                lightningVFX.CreateLightningChain(start, end);
            }
            
            PlayVFX(spawnedVFX);
        }

        private void ApplyLightningDamage()
        {
            // 큐가 비어있는지 체크
            if (targetQueue.Count == 0)
            {
                Debug.LogWarning("targetQueue가 비어있습니다. 번개 연쇄를 종료합니다.");
                chainState = LightningChainState.Finished;
                return;
            }

            // 제일 가까운 적 1명에게 데미지 적용
            Enemy targetEnemy = targetQueue.Peek() as Enemy;
            
            // 파괴된 객체 체크
            if (targetEnemy is null || targetEnemy.transform == null)
            {
                Debug.LogWarning("타겟 Enemy가 파괴되었습니다. 큐에서 제거합니다.");
                targetQueue.Dequeue(); // 파괴된 객체 제거
                ApplyLightningDamage(); // 재귀적으로 다음 타겟 처리
                return;
            }
            
            AttackResult result = new AttackResult();
            result.attack = attack;
            result.attacker = attack.attacker;
            result.totalDamage = chainDamage;
            targetEnemy.ApplyDamage(result);

            // 번개 VFX 생성
            CreateLightningVFX(currentChainPosition, targetEnemy.transform.position);
            
            // 현재 위치 업데이트
            currentChainPosition = targetEnemy.transform.position;
            chainPositions.Add(currentChainPosition);
            
            // 큐에서 제거
            targetQueue.Dequeue();
        }

        /// <summary>
        /// VFX를 생성하고 설정합니다.
        /// </summary>
        /// <param name="vfxPrefab">VFX 프리팹</param>
        /// <param name="position">VFX 생성 위치</param>
        /// <param name="direction">VFX 방향</param>
        /// <returns>생성된 VFX 게임오브젝트</returns>
        protected override GameObject CreateAndSetupVFX(GameObject vfxPrefab, Vector2 position, Vector2 direction)
        {
            // 프리팹이 없으면 VFX 없이 진행
            if (vfxPrefab == null)
            {
                return null;
            }

            // 기본 VFX 생성 (base 호출)
            if (spawnedVFX is null)
            {
                spawnedVFX = base.CreateAndSetupVFX(vfxPrefab, position, direction);
            }
            
            spawnedVFX.transform.position = position;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            spawnedVFX.transform.rotation = Quaternion.Euler(0, 0, angle);
            spawnedVFX.transform.localScale = new Vector3(1.0f, 1.0f, 1f);
            
            spawnedVFX.SetActive(true);
            return spawnedVFX;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            StopAndDestroyVFX(spawnedVFX);
        }
    }

    /// <summary>
    /// 번개 VFX 컴포넌트 (GC 최적화 버전)
    /// </summary>
    public class LightningVFX : MonoBehaviour
    {
        [Header("번개 설정")]
        public LineRenderer lightningLine;
        public float lightningDuration = 0.2f;
        public int maxPoints = 10;
        public float zigzagAmount = 0.5f;
        public Color lightningColor = Color.yellow;

        // GC 최적화를 위한 재사용 배열
        private Vector3[] lightningPoints;
        private float currentDuration = 0f;
        private bool isActive = false;
        private Vector2 startPos, endPos;

        private void Awake()
        {
            // 배열 미리 할당
            lightningPoints = new Vector3[maxPoints];

            // LineRenderer 설정
            lightningLine = gameObject.GetComponent<LineRenderer>();
            if (lightningLine == null)
            {
                lightningLine = gameObject.AddComponent<LineRenderer>();
            }

            lightningLine.material = new Material(Shader.Find("Sprites/Default"));
            lightningLine.startColor = lightningColor;
            lightningLine.endColor = lightningColor;
            lightningLine.startWidth = 0.1f;
            lightningLine.endWidth = 0.05f;
            lightningLine.positionCount = maxPoints;
        }

        public void CreateLightningChain(Vector2 start, Vector2 end)
        {
            startPos = start;
            endPos = end;
            currentDuration = 0f;
            isActive = true;

            // 즉시 첫 번째 번개 생성
            UpdateLightningPath();
            lightningLine.enabled = true;
        }

        private void Update()
        {
            if (!isActive) return;

            currentDuration += Time.deltaTime;

            if (currentDuration < lightningDuration)
            {
                // 번개 경로 업데이트
                UpdateLightningPath();
            }
            else
            {
                // 번개 효과 종료
                isActive = false;
                lightningLine.enabled = false;
            }
        }

        private void UpdateLightningPath()
        {
            // 기존 배열 재사용하여 GC 방지
            for (int i = 0; i < maxPoints; i++)
            {
                float t = (float)i / (maxPoints - 1);
                Vector2 basePos = Vector2.Lerp(startPos, endPos, t);

                // 지그재그 효과 (시간에 따른 변화)
                float timeOffset = currentDuration * 10f;
                Vector2 zigzag = new Vector2(
                    Mathf.Sin(t * 20f + timeOffset) * zigzagAmount,
                    Mathf.Cos(t * 15f + timeOffset) * zigzagAmount
                );

                lightningPoints[i] = basePos + zigzag;
            }

            lightningLine.SetPositions(lightningPoints);
        }
    }
} 