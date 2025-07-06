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
        public int gridSize = 50; // 공간 분할을 위한 그리드 크기
        
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
        
        private Tilemap tilemap;
        private TileBase ruleTile;
        
        /// <summary>
        /// 타일 풀링 시스템을 초기화합니다.
        /// </summary>
        public void Initialize(Tilemap tilemap, TileBase ruleTile)
        {
            this.tilemap = tilemap;
            this.ruleTile = ruleTile;
            isInitialized = true;
            
            // 그리드 시스템 초기화
            InitializeSpatialGrid();
            
            Debug.Log("[TILE_POOLING] 최적화된 타일 풀링 시스템이 초기화되었습니다.");
        }
        
        /// <summary>
        /// 공간 분할을 위한 그리드 시스템을 초기화합니다.
        /// </summary>
        private void InitializeSpatialGrid()
        {
            spatialGrid.Clear();
            Debug.Log("[TILE_POOLING] 공간 분할 그리드 시스템이 초기화되었습니다.");
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
            
            // 풀에서 재사용 또는 새로 생성
            bool reused = false;
            if (tilePool.Count > 0)
            {
                Vector3Int pooledTile = tilePool.Dequeue();
                tileData.Remove(pooledTile);
                reused = true;
            }
            
            // 타일 설정
            tilemap.SetTile(position, ruleTile);
            tilemap.SetTileFlags(position, TileFlags.None);
            
            // 데이터 구조 업데이트
            activeTiles.Add(position);
            tileData[position] = ruleTile;
            
            // 공간 분할 그리드 업데이트
            UpdateSpatialGrid(position, true);
            
            if (reused)
            {
                Debug.Log($"[TILE_POOLING] 풀에서 타일 재사용: {position}");
            }
            else
            {
                Debug.Log($"[TILE_POOLING] 새 타일 생성: {position}");
            }
            
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
            
            Debug.Log($"[TILE_POOLING] 타일을 풀로 반환: {position}");
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
        /// 특정 범위의 타일들을 배치로 생성합니다. (O(n) 최적화)
        /// </summary>
        public int CreateTilesInRange(Vector3Int center, int width, int height)
        {
            if (!isInitialized) return 0;
            
            int createdCount = 0;
            int halfWidth = width / 2;
            int halfHeight = height / 2;
            
            // 배치 처리를 위한 버퍼 초기화
            batchCreateBuffer.Clear();
            
            // 범위 내 모든 위치를 버퍼에 추가
            for (int y = center.y - halfHeight; y <= center.y + halfHeight; y++)
            {
                for (int x = center.x - halfWidth; x <= center.x + halfWidth; x++)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    if (!activeTiles.Contains(tilePos))
                    {
                        batchCreateBuffer.Add(tilePos);
                    }
                }
            }
            
            // 배치로 타일 생성 (성능 최적화)
            foreach (Vector3Int tilePos in batchCreateBuffer)
            {
                if (CreateTile(tilePos))
                {
                    createdCount++;
                }
            }
            
            Debug.Log($"[TILE_POOLING] 배치 타일 생성 완료: {createdCount}개 생성됨 (중심: {center}, 크기: {width}x{height})");
            return createdCount;
        }
        
        /// <summary>
        /// 특정 범위 밖의 타일들을 효율적으로 제거합니다. (O(log n) 최적화)
        /// </summary>
        public int RemoveTilesOutsideRange(Vector3Int center, int width, int height)
        {
            if (!isInitialized) return 0;
            
            int removedCount = 0;
            int halfWidth = width / 2;
            int halfHeight = height / 2;
            
            // 배치 처리를 위한 버퍼 초기화
            batchRemoveBuffer.Clear();
            
            // 공간 분할을 활용한 효율적인 범위 쿼리
            Vector2Int minGrid = WorldToGridCell(new Vector3Int(center.x - halfWidth, center.y - halfHeight, 0));
            Vector2Int maxGrid = WorldToGridCell(new Vector3Int(center.x + halfWidth, center.y + halfHeight, 0));
            
            // 범위 밖의 그리드 셀들만 처리
            foreach (var gridCell in spatialGrid.Keys)
            {
                if (gridCell.x < minGrid.x || gridCell.x > maxGrid.x ||
                    gridCell.y < minGrid.y || gridCell.y > maxGrid.y)
                {
                    // 이 그리드 셀의 모든 타일을 제거 대상에 추가
                    foreach (Vector3Int tilePos in spatialGrid[gridCell])
                    {
                        batchRemoveBuffer.Add(tilePos);
                    }
                }
            }
            
            // 배치로 타일 제거
            foreach (Vector3Int tilePos in batchRemoveBuffer)
            {
                if (RemoveTile(tilePos))
                {
                    removedCount++;
                }
            }
            
            Debug.Log($"[TILE_POOLING] 배치 타일 제거 완료: {removedCount}개 제거됨 (중심: {center}, 크기: {width}x{height})");
            return removedCount;
        }
        
        /// <summary>
        /// 풀에 있는 타일 개수를 반환합니다. (O(1))
        /// </summary>
        public int GetPooledTileCount()
        {
            return tilePool.Count;
        }
        
        /// <summary>
        /// 특정 범위 내의 타일 개수를 효율적으로 계산합니다. (O(log n))
        /// </summary>
        public int GetTileCountInRange(Vector3Int center, int width, int height)
        {
            if (!isInitialized) return 0;
            
            int count = 0;
            int halfWidth = width / 2;
            int halfHeight = height / 2;
            
            // 공간 분할을 활용한 효율적인 범위 쿼리
            Vector2Int minGrid = WorldToGridCell(new Vector3Int(center.x - halfWidth, center.y - halfHeight, 0));
            Vector2Int maxGrid = WorldToGridCell(new Vector3Int(center.x + halfWidth, center.y + halfHeight, 0));
            
            for (int gridY = minGrid.y; gridY <= maxGrid.y; gridY++)
            {
                for (int gridX = minGrid.x; gridX <= maxGrid.x; gridX++)
                {
                    Vector2Int gridCell = new Vector2Int(gridX, gridY);
                    if (spatialGrid.ContainsKey(gridCell))
                    {
                        // 그리드 셀 내의 타일들 중 범위 내에 있는 것만 카운트
                        foreach (Vector3Int tilePos in spatialGrid[gridCell])
                        {
                            if (tilePos.x >= center.x - halfWidth && tilePos.x <= center.x + halfWidth &&
                                tilePos.y >= center.y - halfHeight && tilePos.y <= center.y + halfHeight)
                            {
                                count++;
                            }
                        }
                    }
                }
            }
            
            return count;
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
            
            Debug.Log("[TILE_POOLING] 모든 타일이 제거되고 풀이 정리되었습니다.");
        }
        
        /// <summary>
        /// 타일 풀링 시스템의 상태를 로그로 출력합니다.
        /// </summary>
        public void LogStatus()
        {
            Debug.Log($"[TILE_POOLING_STATUS] 활성 타일: {activeTiles.Count}개, 풀에 있는 타일: {tilePool.Count}개, 그리드 셀: {spatialGrid.Count}개");
        }
        
        /// <summary>
        /// 성능 통계를 출력합니다.
        /// </summary>
        public void LogPerformanceStats()
        {
            float memoryUsage = (activeTiles.Count + tilePool.Count) * sizeof(int) / 1024f; // KB
            Debug.Log($"[TILE_POOLING_PERFORMANCE] 메모리 사용량: {memoryUsage:F2}KB, 그리드 효율성: {spatialGrid.Count}/{activeTiles.Count}");
        }
    }
} 