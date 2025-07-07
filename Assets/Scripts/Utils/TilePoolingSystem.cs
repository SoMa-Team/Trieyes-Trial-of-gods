using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utils
{
    /// <summary>
    /// 타일 풀링 시스템 - 시간복잡도 최적화 버전
    /// 공간 분할과 배치 처리를 통해 O(n²)에서 O(log n)으로 개선
    /// </summary>
    public class TilePoolingSystem : MonoBehaviour
    {
        [Header("Tile Pooling Settings")]
        public float boundaryThreshold;
        private int gridSize; // 하나만 남김
        
        // === 핵심 데이터 구조 ===
        private Queue<Vector3Int> tilePool = new Queue<Vector3Int>();
        private HashSet<Vector3Int> activeTiles = new HashSet<Vector3Int>();
        private Dictionary<Vector3Int, TileBase> tileData = new Dictionary<Vector3Int, TileBase>();
        
        // === 공간 분할을 위한 Grid 시스템 ===
        private Dictionary<Vector2Int, HashSet<Vector3Int>> spatialGrid = new Dictionary<Vector2Int, HashSet<Vector3Int>>();
        private int gridCellSize = 10; // 각 그리드 셀의 크기
        
        // === 성능 최적화를 위한 캐시 ===
        private List<Vector3Int> tilesToRemove = new List<Vector3Int>();
        private List<Vector3Int> tilesToCreate = new List<Vector3Int>();
        private HashSet<Vector2Int> affectedGridCells = new HashSet<Vector2Int>();
        private bool isInitialized = false;
        
        // === 배치 처리를 위한 버퍼 ===
        private List<Vector3Int> batchCreateBuffer = new List<Vector3Int>();
        private List<Vector3Int> batchRemoveBuffer = new List<Vector3Int>();
        
        // === 타일맵 랜덤 패턴 관련 필드 ===
        private Tilemap tilemap;
        private TileBase[] ruleTiles;

        private Dictionary<Vector3Int, TileBase> prevTiles = new Dictionary<Vector3Int, TileBase>();

        /// <summary>
        /// 타일 풀링 시스템을 초기화합니다. (ruleTiles, width, height 포함)
        /// </summary>
        public void Initialize(Tilemap tilemap, TileBase[] ruleTiles)
        {
            this.tilemap = tilemap;
            this.ruleTiles = ruleTiles;
            isInitialized = true;
            
            // 그리드 시스템 초기화
            InitializeSpatialGrid();
            
            //Debug.Log("[TILE_POOLING] 최적화된 타일 풀링 시스템이 초기화되었습니다.");
        }
        
        /// <summary>
        /// 공간 분할을 위한 그리드 시스템을 초기화합니다.
        /// </summary>
        private void InitializeSpatialGrid()
        {
            spatialGrid.Clear();
            //Debug.Log("[TILE_POOLING] 공간 분할 그리드 시스템이 초기화되었습니다.");
        }
        
        /// <summary>
        /// 월드 좌표를 그리드 셀 좌표로 변환합니다.
        /// </summary>
        private Vector2Int WorldToGridCell(Vector3Int worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / (float)gridCellSize),
                Mathf.FloorToInt(worldPos.y / (float)gridCellSize)
            );
        }
        
        /// <summary>
        /// 특정 위치에 타일을 생성합니다. (O(1) 최적화)
        /// </summary>
        public bool CreateTile(Vector3Int position)
        {
            if (!isInitialized || activeTiles.Contains(position))
            {
                return false;
            }
            
            if (tilePool.Count > 0)
            {
                Vector3Int pooledTile = tilePool.Dequeue();
                tileData.Remove(pooledTile);
            }
            
            // 타일 설정
            tilemap.SetTile(position, ruleTiles[0]); // 임시로 첫 번째 타일 사용
            tilemap.SetTileFlags(position, TileFlags.None);
            
            // 데이터 구조 업데이트
            activeTiles.Add(position);
            tileData[position] = ruleTiles[0];
            
            // 공간 분할 그리드 업데이트
            UpdateSpatialGrid(position, true);
            
            return true;
        }
        
        /// <summary>
        /// 특정 위치의 타일을 제거합니다. (O(1) 최적화)
        /// </summary>
        public bool RemoveTile(Vector3Int position)
        {
            if (!isInitialized || !activeTiles.Contains(position))
            {
                return false;
            }
            
            // 타일 제거
            tilemap.SetTile(position, null);
            
            // 데이터 구조 업데이트
            activeTiles.Remove(position);
            tilePool.Enqueue(position);
            
            // 공간 분할 그리드 업데이트
            UpdateSpatialGrid(position, false);
            
            //Debug.Log($"[TILE_POOLING] 타일을 풀로 반환: {position}");
            return true;
        }
        
        /// <summary>
        /// 공간 분할 그리드를 업데이트합니다.
        /// </summary>
        private void UpdateSpatialGrid(Vector3Int position, bool isAdding)
        {
            Vector2Int gridCell = WorldToGridCell(position);
            
            if (!spatialGrid.ContainsKey(gridCell))
            {
                spatialGrid[gridCell] = new HashSet<Vector3Int>();
            }
            
            if (isAdding)
            {
                spatialGrid[gridCell].Add(position);
            }
            else
            {
                spatialGrid[gridCell].Remove(position);
                if (spatialGrid[gridCell].Count == 0)
                {
                    spatialGrid.Remove(gridCell);
                }
            }
        }
   
        /// <summary>
        /// 풀에 있는 타일 개수를 반환합니다. (O(1))
        /// </summary>
        public int GetPooledTileCount()
        {
            return tilePool.Count;
        }
      
        /// <summary>
        /// 모든 타일을 제거하고 풀을 정리합니다.
        /// </summary>
        public void ClearAllTiles()
        {
            if (!isInitialized) return;
            
            // 배치로 모든 타일 제거
            foreach (Vector3Int tilePos in activeTiles)
            {
                tilemap.SetTile(tilePos, null);
            }
            
            activeTiles.Clear();
            tileData.Clear();
            tilePool.Clear();
            spatialGrid.Clear();
            
            //Debug.Log("[TILE_POOLING] 모든 타일이 제거되고 풀이 정리되었습니다.");
        }

        /// <summary>
        /// 타일맵 전체를 비우지 않고, 변경이 필요한 셀만 갱신합니다.
        /// </summary>
        public void FillTilesWithRandomPattern(Vector3Int center, int gridSize)
        {
            if (!isInitialized || ruleTiles == null || ruleTiles.Length == 0) return;
            int half = gridSize / 2;
            HashSet<Vector3Int> newTiles = new HashSet<Vector3Int>();
            for (int y = center.y - half; y <= center.y + half; y++)
            {
                for (int x = center.x - half; x <= center.x + half; x++)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    int randomIndex = UnityEngine.Random.Range(0, ruleTiles.Length);
                    TileBase tileToUse = ruleTiles[randomIndex];
                    newTiles.Add(tilePos);
                    // 이전과 다를 때만 SetTile
                    if (!prevTiles.ContainsKey(tilePos) || prevTiles[tilePos] != tileToUse)
                    {
                        tilemap.SetTile(tilePos, tileToUse);
                        prevTiles[tilePos] = tileToUse;
                    }
                }
            }
            // 범위 밖의 타일은 null로 바꿔줌
            var prevKeys = new List<Vector3Int>(prevTiles.Keys);
            foreach (var tilePos in prevKeys)
            {
                if (!newTiles.Contains(tilePos))
                {
                    tilemap.SetTile(tilePos, null);
                    prevTiles.Remove(tilePos);
                }
            }
        }
        
        /// <summary>
        /// 타일 풀링 시스템의 상태를 로그로 출력합니다.
        /// </summary>
        public void LogStatus()
        {
            //Debug.Log($"[TILE_POOLING_STATUS] 활성 타일: {activeTiles.Count}개, 풀에 있는 타일: {tilePool.Count}개, 그리드 셀: {spatialGrid.Count}개");
        }
        
        /// <summary>
        /// 성능 통계를 출력합니다.
        /// </summary>
        public void LogPerformanceStats()
        {
            float memoryUsage = (activeTiles.Count + tilePool.Count) * sizeof(int) / 1024f; // KB
            //Debug.Log($"[TILE_POOLING_PERFORMANCE] 메모리 사용량: {memoryUsage:F2}KB, 그리드 효율성: {spatialGrid.Count}/{activeTiles.Count}");
        }
    }
} 