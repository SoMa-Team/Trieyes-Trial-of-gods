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
    public class AC101_CHAIN : AttackComponent
    {
        [Header("번개 연쇄 설정")]
        public float chainDamage;
        public float chainRadius; // 번개가 전염되는 최대 거리
        public int chainCount; // 번개 전염 횟수
        public float chainDelay; // 연쇄 간격

        [Header("VFX 설정")]
        public GameObject lightningVFXPrefab;
        public float lightningDuration = 0.2f;

        // 번개 연쇄 상태 관리
        private LightningChainState chainState = LightningChainState.None;
        private float chainTimer = 0f;
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
            ApplyLightningDamage(startPosition);
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
                    // 연쇄 처리
                    chainTimer += Time.deltaTime;

                    if (chainTimer >= chainDelay)
                    {
                        ProcessNextChain();
                        chainTimer = 0f;
                    }
                    break;

                case LightningChainState.Finished:
                    // 연쇄 완료
                    chainState = LightningChainState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void ProcessNextChain()
        {
            if (currentChainCount >= chainCount || targetQueue.Count == 0)
            {
                // 최대 전염 횟수 도달 또는 더 이상 타겟이 없으면 종료
                chainState = LightningChainState.Finished;
                return;
            }

            // 큐에서 다음 타겟을 가져옴
            Pawn nextTarget = targetQueue.Dequeue();
            Vector2 nextPos = nextTarget.transform.position;

            // 번개 VFX 생성
            CreateLightningVFX(currentChainPosition, nextPos);

            // 데미지 적용
            ApplyLightningDamage(nextPos);

            // 다음 연쇄 준비
            currentChainPosition = nextPos;
            chainPositions.Add(nextPos);
            currentChainCount++;
        }

        private void InitializeTargetQueue(Vector2 startPosition)
        {
            // 큐 초기화
            targetQueue.Clear();

            // 초기 범위에서 모든 적을 찾기
            reusableColliders.Clear();
            Physics2D.OverlapCircle(startPosition, chainRadius, new ContactFilter2D().NoFilter(), reusableColliders);

            // Pawn들을 거리 순으로 정렬할 리스트
            List<(Pawn pawn, float distance)> sortedTargets = new List<(Pawn, float)>();

            // 모든 적을 거리와 함께 수집 (Controller가 없는 Pawn만 - 즉, 적만)
            for (int i = 0; i < reusableColliders.Count; i++)
            {
                Collider2D collider = reusableColliders[i];
                if (collider == null) continue;

                Pawn enemy = collider.GetComponent<Pawn>();
                if (enemy != null && enemy.GetComponent<Controller>() is EnemyController)
                {
                    float distance = Vector2.Distance(startPosition, enemy.transform.position);
                    sortedTargets.Add((enemy, distance));
                }
            }

            // 거리 순으로 정렬 (가까운 순)
            sortedTargets.Sort((a, b) => a.distance.CompareTo(b.distance));

            // chainCount만큼만 큐에 추가
            int targetCount = Mathf.Min(chainCount, sortedTargets.Count);
            for (int i = 0; i < targetCount; i++)
            {
                targetQueue.Enqueue(sortedTargets[i].pawn);
            }

            Debug.Log($"targetQueue.Count: {targetQueue.Count}");
            foreach (var target in targetQueue)
            {
                Debug.Log($"targetQueue: {target.pawnName}, Position: {target.transform.position}");
            }
        }



        private void CreateLightningVFX(Vector2 start, Vector2 end)
        {
            // VFX 오브젝트 생성 (실제 구현에서는 오브젝트 풀링 사용 권장)
            if (lightningVFXPrefab != null)
            {
                GameObject vfxObject = Instantiate(lightningVFXPrefab);
                vfxObject.transform.position = (start + end) * 0.5f; // 중간 위치

                // LightningVFX 컴포넌트 찾아서 번개 효과 시작
                LightningVFX lightningVFX = vfxObject.GetComponent<LightningVFX>();
                if (lightningVFX != null)
                {
                    lightningVFX.CreateLightningChain(start, end);
                }

                // 일정 시간 후 VFX 오브젝트 제거
                Destroy(vfxObject, lightningDuration + 0.1f);
            }
        }

        private void ApplyLightningDamage(Vector2 position)
        {
            // 위치에서 Pawn 찾기
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.5f);
            foreach (var collider in colliders)
            {
                Pawn target = collider.GetComponent<Pawn>();
                if (target != null)
                {
                    AttackResult result = new AttackResult();
                    result.attacker = attack.attacker;
                    result.totalDamage = (int)chainDamage;
                    target.ApplyDamage(result);
                    break; // 첫 번째 Pawn에만 데미지 적용
                }
            }
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