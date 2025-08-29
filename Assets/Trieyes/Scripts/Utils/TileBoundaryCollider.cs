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
        
        private BoxCollider2D _boundaryCollider;
        
        /// <summary>
        /// 타일 경계 콜리전을 초기화합니다.
        /// </summary>
        /// <param name="view">BattleStageView 참조</param>
        /// <param name="thickness">경계 콜리전의 두께</param>
        public void Awake()
        {
            _boundaryCollider = GetComponent<BoxCollider2D>();
        }
    }
}
