using UnityEngine;
using BattleSystem;

namespace Utils
{
    /// <summary>
    /// 타일 경계를 감지하는 콜리전 시스템
    /// 사각형 콜리전을 사용하여 경계에 도달했을 때만 타일 업데이트를 트리거합니다.
    /// </summary>
    public class TileBoundaryCollider : MonoBehaviour
    {
        [Header("Boundary Settings")]
        public float boundaryThreshold; // 경계 임계값
        public int tileWidth;
        public int tileHeight;
        
        private BoxCollider2D boundaryCollider;
        private BattleStageView battleStageView;
        private Vector3Int lastTileCenter;
        private bool isInitialized = false;
        
        /// <summary>
        /// 타일 경계 콜리전을 초기화합니다.
        /// </summary>
        /// <param name="view">BattleStageView 참조</param>
        public void Initialize(BattleStageView view)
        {
            battleStageView = view;
            lastTileCenter = Vector3Int.zero;
            
            // BoxCollider2D 컴포넌트 추가
            boundaryCollider = gameObject.AddComponent<BoxCollider2D>();
            boundaryCollider.isTrigger = true; // 트리거로 설정
            
            // 콜리전 크기 설정 (경계 임계값 적용)
            float colliderWidth = tileWidth * boundaryThreshold;
            float colliderHeight = tileHeight * boundaryThreshold;
            boundaryCollider.size = new Vector2(colliderWidth, colliderHeight);
            
            // 초기 위치 설정
            transform.position = Vector3.zero;
            
            isInitialized = true;
            
            Debug.Log($"[TILE_BOUNDARY] 타일 경계 콜리전 초기화 완료 (크기: {colliderWidth}x{colliderHeight})");
        }
        
        /// <summary>
        /// 콜리전이 트리거될 때 호출됩니다.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isInitialized || battleStageView == null) return;
            
            // 플레이어 캐릭터인지 확인
            if (other.CompareTag("Player") || other.GetComponent<CharacterSystem.Pawn>() != null)
            {
                HandleBoundaryTrigger(other.transform.position);
            }
        }
        
        /// <summary>
        /// 콜리전이 트리거 상태일 때 호출됩니다.
        /// </summary>
        private void OnTriggerStay2D(Collider2D other)
        {
            if (!isInitialized || battleStageView == null) return;
            
            // 플레이어 캐릭터인지 확인
            if (other.CompareTag("Player") || other.GetComponent<CharacterSystem.Pawn>() != null)
            {
                HandleBoundaryTrigger(other.transform.position);
            }
        }
        
        /// <summary>
        /// 경계 트리거를 처리합니다.
        /// </summary>
        private void HandleBoundaryTrigger(Vector3 characterPosition)
        {
            // 캐릭터 위치를 타일 좌표로 변환
            Vector3Int currentTileCenter = new Vector3Int(
                Mathf.RoundToInt(characterPosition.x),
                Mathf.RoundToInt(characterPosition.y),
                0
            );
            
            // 새로운 중심과 마지막 중심의 거리 계산
            int deltaX = Mathf.Abs(currentTileCenter.x - lastTileCenter.x);
            int deltaY = Mathf.Abs(currentTileCenter.y - lastTileCenter.y);
            int distanceFromLastCenter = Mathf.Max(deltaX, deltaY);
            
            // 경계 거리 계산
            int boundaryDistance = Mathf.RoundToInt(Mathf.Min(tileWidth, tileHeight) * boundaryThreshold / 2f);
            
            // 경계에 도달했는지 확인
            if (distanceFromLastCenter >= boundaryDistance)
            {
                Debug.Log($"[TILE_BOUNDARY] 경계 트리거! 캐릭터 위치: {characterPosition}, 새로운 중심: {currentTileCenter}");
                
                // 타일 업데이트 수행
                battleStageView.UpdateTilesAtBoundary(currentTileCenter);
                
                // 마지막 중심 업데이트
                lastTileCenter = currentTileCenter;
                
                // 콜리전 위치 업데이트
                UpdateColliderPosition(currentTileCenter);
            }
        }
        
        /// <summary>
        /// 콜리전 위치를 업데이트합니다.
        /// </summary>
        private void UpdateColliderPosition(Vector3Int tileCenter)
        {
            transform.position = new Vector3(tileCenter.x, tileCenter.y, 0);
            Debug.Log($"[TILE_BOUNDARY] 콜리전 위치 업데이트: {transform.position}");
        }
        
        /// <summary>
        /// 콜리전 크기를 업데이트합니다.
        /// </summary>
        public void UpdateColliderSize(int width, int height)
        {
            if (boundaryCollider == null) return;
            
            tileWidth = width;
            tileHeight = height;
            
            float colliderWidth = tileWidth * boundaryThreshold;
            float colliderHeight = tileHeight * boundaryThreshold;
            boundaryCollider.size = new Vector2(colliderWidth, colliderHeight);
            
            Debug.Log($"[TILE_BOUNDARY] 콜리전 크기 업데이트: {colliderWidth}x{colliderHeight}");
        }
        
        /// <summary>
        /// 경계 임계값을 업데이트합니다.
        /// </summary>
        public void UpdateBoundaryThreshold(float threshold)
        {
            boundaryThreshold = threshold;
            UpdateColliderSize(tileWidth, tileHeight);
        }
        
        /// <summary>
        /// 현재 콜리전 상태를 로그로 출력합니다.
        /// </summary>
        public void LogStatus()
        {
            if (boundaryCollider != null)
            {
                Debug.Log($"[TILE_BOUNDARY_STATUS] 위치: {transform.position}, 크기: {boundaryCollider.size}, 임계값: {boundaryThreshold}");
            }
        }
    }
} 