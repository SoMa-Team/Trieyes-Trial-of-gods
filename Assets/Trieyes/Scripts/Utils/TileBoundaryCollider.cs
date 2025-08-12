using UnityEngine;
using BattleSystem;

namespace Utils
{
    /// <summary>
    /// 타일 경계를 감지하는 물리적 콜리전 시스템
    /// 플레이어와 적을 물리적으로 가두는 역할만 수행합니다.
    /// </summary>
    public class TileBoundaryCollider : MonoBehaviour
    {
        [Header("Boundary Settings")]
        
        private Vector2Int gridSize;
        private BoxCollider2D boundaryCollider;
        private BattleStageView battleStageView;
        private bool isInitialized = false;
        
        /// <summary>
        /// 타일 경계 콜리전을 초기화합니다.
        /// </summary>
        /// <param name="view">BattleStageView 참조</param>
        /// <param name="thickness">경계 콜리전의 두께</param>
        public void Initialize(BattleStageView view, float thickness = 1f)
        {
            battleStageView = view;
            
            // 타일맵 크기 가져오기
            gridSize = battleStageView.GetTilemapSize();
            
            // BoxCollider2D 컴포넌트 추가
            boundaryCollider = gameObject.AddComponent<BoxCollider2D>();
            boundaryCollider.isTrigger = false; // 물리적 콜리전으로 설정
            
            // 콜리전 크기 설정 (타일맵 크기 + 경계 두께)
            float colliderWidth = gridSize.x;
            float colliderHeight = gridSize.y;
            boundaryCollider.size = new Vector2(colliderWidth, colliderHeight);
            
            // 초기 위치 설정 (타일맵 중심)
            transform.position = Vector3.zero;
            
            isInitialized = true;
        }
    }
}
