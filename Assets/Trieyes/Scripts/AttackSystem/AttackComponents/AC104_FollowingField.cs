using AttackSystem;
using CharacterSystem;
using CharacterSystem.Enemies;
using Stats;
using UnityEngine;
using System.Collections.Generic;
using BattleSystem;

namespace AttackComponents
{
    /// <summary>
    /// 플레이어를 따라다니는 자기장 효과
    /// 플레이어 주변에 자기장을 형성하여 닿는 적에게 피해를 주고, 이동속도가 증가합니다.
    /// 플레이어가 이동하면 자기장도 함께 따라다닙니다.
    /// BattleStage 기반 적 감지로 최적화된 성능을 제공합니다.
    /// </summary>
    public class AC104_FollowingField : AttackComponent
    {
        public enum FieldShape { Circle, Rect }

        [Header("필드 타입 및 크기 설정")]
        public FieldShape fieldShape = FieldShape.Circle;
        public float fieldRadius = 2.5f; // 원형일 때 반지름
        public float fieldWidth = 3f;    // 네모일 때 가로
        public float fieldHeight = 3f;   // 네모일 때 세로
        public float fieldDamage = 30f;
        public float fieldTickInterval = 0.5f;
        public float fieldDuration = 5f;
        
        [Header("따라다니기 설정")]
        public float followDistance = 0f; // 플레이어로부터의 거리 (0이면 플레이어 위치)
        public bool followPlayer = true;
        public Vector2 followOffset = Vector2.zero; // 추가 오프셋
        
        [Header("버프 설정")]
        
        [Header("VFX 설정")]
        public GameObject fieldVFXPrefab;
        public float fieldVFXDuration = 0.3f;
        
        // 자기장 상태 관리
        private FollowingFieldState fieldState = FollowingFieldState.None;
        private float fieldTimer = 0f;
        private float damageTimer = 0f;
        private List<Pawn> fieldTargets = new List<Pawn>(10); // 재사용 가능한 리스트
        private bool buffApplied = false;
        private GameObject activeFieldVFX;
        
        // 자기장 상태 열거형
        private enum FollowingFieldState
        {
            None,
            Starting,
            Active,
            Ending,
            Finished
        }
        
        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            // 초기 상태 설정
            fieldState = FollowingFieldState.None;
            fieldTimer = 0f;
            damageTimer = 0f;
            fieldTargets.Clear();
            buffApplied = false;
            
            // 자기장 시작
            StartFollowingField();
        }
        
        private void StartFollowingField()
        {
            fieldState = FollowingFieldState.Starting;
            fieldTimer = 0f;
            damageTimer = 0f;
            
            // 초기 위치 설정
            UpdateFieldPosition();
            
            // VFX 생성
            CreateFieldVFX();
            
            Debug.Log("<color=cyan>[AC104] 따라다니는 자기장 시작!</color>");
        }
        
        protected override void Update()
        {
            base.Update();
            
            // 자기장 처리
            ProcessFollowingField();
        }
        
        private void ProcessFollowingField()
        {
            switch (fieldState)
            {
                case FollowingFieldState.None:
                    break;
                    
                case FollowingFieldState.Starting:
                    fieldTimer += Time.deltaTime;
                    
                    if (fieldTimer >= 0.1f) // 시작 지연
                    {
                        fieldState = FollowingFieldState.Active;
                        fieldTimer = 0f;
                        ActivateField();
                    }
                    break;
                    
                case FollowingFieldState.Active:
                    fieldTimer += Time.deltaTime;
                    damageTimer += Time.deltaTime;
                    
                    // 플레이어 위치로 자기장 이동
                    UpdateFieldPosition();
                    
                    // 데미지 처리
                    if (damageTimer >= fieldTickInterval)
                    {
                        ApplyFieldDamage();
                        damageTimer = 0f;
                    }
                    
                    // 지속시간 체크
                    if (fieldTimer >= fieldDuration)
                    {
                        fieldState = FollowingFieldState.Ending;
                        fieldTimer = 0f;
                        DeactivateField();
                    }
                    break;
                    
                case FollowingFieldState.Ending:
                    fieldTimer += Time.deltaTime;
                    
                    if (fieldTimer >= fieldVFXDuration)
                    {
                        fieldState = FollowingFieldState.Finished;
                    }
                    break;
                    
                case FollowingFieldState.Finished:
                    fieldState = FollowingFieldState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }
        
        private void UpdateFieldPosition()
        {
            if (!followPlayer || attack.attacker == null) return;
            
            Vector2 playerPosition = attack.attacker.transform.position;
            Vector2 fieldPosition = playerPosition + followOffset;
            
            // followDistance가 0이 아닌 경우 방향 벡터를 이용해 오프셋 계산
            if (followDistance > 0f)
            {
                Vector2 direction = attack.attacker.transform.position - transform.position;
                fieldPosition += direction * followDistance;
            }
            
            transform.position = fieldPosition;
            
            // VFX 위치도 업데이트
            if (activeFieldVFX != null)
            {
                activeFieldVFX.transform.position = fieldPosition;
            }

            // 디버그 용으로 필드를 Draw
            // if (fieldShape == FieldShape.Circle)
            // {
            //     // 원형 필드 디버그 (원의 둘레를 그리기)
            //     int segments = 16;
            //     Vector2 prevPoint = fieldPosition + Vector2.right * fieldRadius;
            //     for (int i = 1; i <= segments; i++)
            //     {
            //         float angle = (360f / segments) * i * Mathf.Deg2Rad;
            //         Vector2 currentPoint = fieldPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * fieldRadius;
            //         Debug.DrawLine(prevPoint, currentPoint, Color.red, 1f);
            //         prevPoint = currentPoint;
            //     }
            // }
            // else if (fieldShape == FieldShape.Rect)
            // {
            //     // 사각형 필드 디버그 (네 개의 모서리를 연결)
            //     Vector2 halfSize = new Vector2(fieldWidth, fieldHeight) * 0.5f;
            //     Vector2 topLeft = fieldPosition - halfSize;
            //     Vector2 topRight = fieldPosition + new Vector2(halfSize.x, -halfSize.y);
            //     Vector2 bottomLeft = fieldPosition + new Vector2(-halfSize.x, halfSize.y);
            //     Vector2 bottomRight = fieldPosition + halfSize;
                
            //     // 사각형의 네 변을 그리기
            //     Debug.DrawLine(topLeft, topRight, Color.red, 1f);     // 위쪽 변
            //     Debug.DrawLine(topRight, bottomRight, Color.red, 1f); // 오른쪽 변
            //     Debug.DrawLine(bottomRight, bottomLeft, Color.red, 1f); // 아래쪽 변
            //     Debug.DrawLine(bottomLeft, topLeft, Color.red, 1f);   // 왼쪽 변
            // }
        }
        
        private void ActivateField()
        {
            Debug.Log("<color=green>[AC104] 자기장 활성화!</color>");
        }
        
        private void ApplyFieldDamage()
        {
            // 자기장 범위 내 적 탐지 (BattleStage 기반)
            DetectEnemiesInField();
            
            // 탐지된 적들에게 데미지 적용
            for (int i = 0; i < fieldTargets.Count; i++)
            {
                Pawn enemy = fieldTargets[i];
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    ApplyDamageToEnemy(enemy);
                }
            }
        }
        
        private void DetectEnemiesInField()
        {
            fieldTargets.Clear();
            Vector2 fieldPosition = transform.position;
            
            if (BattleStage.now != null && BattleStage.now.enemies != null)
            {
                foreach (var enemy in BattleStage.now.enemies)
                {
                    if (enemy == null || enemy.transform == null || !enemy.gameObject.activeInHierarchy)
                        continue;

                    bool inRange = false;
                    if (fieldShape == FieldShape.Circle)
                    {
                        // 원형 범위
                        float distance = Vector2.Distance(enemy.transform.position, fieldPosition);
                        inRange = (distance <= fieldRadius);
                    }
                    else if (fieldShape == FieldShape.Rect)
                    {
                        // 네모 범위
                        Vector2 min = fieldPosition - new Vector2(fieldWidth, fieldHeight) * 0.5f;
                        Vector2 max = fieldPosition + new Vector2(fieldWidth, fieldHeight) * 0.5f;
                        Vector2 pos = (Vector2)enemy.transform.position;
                        inRange = (pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y);
                    }

                    if (inRange)
                    {
                        if (enemy.GetComponent<Controller>() is EnemyController)
                        {
                            fieldTargets.Add(enemy);
                        }
                    }
                }
            }
            Debug.Log($"<color=blue>[AC104] 자기장 범위 내 적 탐지: {fieldTargets.Count}명</color>");
        }
        
        private void ApplyDamageToEnemy(Pawn enemy)
        {
            var attackResult = AttackResult.Create(attack, enemy);
            attackResult.totalDamage = (int)fieldDamage;
            enemy.ApplyDamage(attackResult);
            
            Debug.Log($"<color=yellow>[AC104] 자기장으로 {enemy.pawnName}에게 {attackResult.totalDamage} 데미지 적용</color>");
        }
        
        private void CreateFieldVFX()
        {
            if (fieldVFXPrefab != null)
            {
                activeFieldVFX = Instantiate(fieldVFXPrefab);
                activeFieldVFX.transform.position = transform.position;
                
                // 자기장 VFX 지속시간 후 제거
                Destroy(activeFieldVFX, fieldVFXDuration);
                
                Debug.Log("<color=blue>[AC104] 자기장 VFX 생성</color>");
            }
        }
        
        private void DeactivateField()
        {
            // VFX 정리
            if (activeFieldVFX != null)
            {
                Destroy(activeFieldVFX);
                activeFieldVFX = null;
            }
            
            Debug.Log("<color=cyan>[AC104] 따라다니는 자기장 종료!</color>");
        }
        
        public override void Deactivate()
        {
            base.Deactivate();
            
            // VFX 정리
            if (activeFieldVFX != null)
            {
                Destroy(activeFieldVFX);
                activeFieldVFX = null;
            }
            
            fieldState = FollowingFieldState.None;
            fieldTimer = 0f;
            damageTimer = 0f;
            fieldTargets.Clear();
            buffApplied = false;
        }
    }
} 