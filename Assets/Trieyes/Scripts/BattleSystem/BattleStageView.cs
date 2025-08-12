using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using CharacterSystem;
using Utils;

namespace BattleSystem
{
    /// <summary>
    /// 전투 스테이지의 뷰 컴포넌트
    /// BattleStage 데이터와 Unity GameObject를 연결하는 역할을 합니다.
    /// </summary>
    public class BattleStageView : MonoBehaviour
    {
        // ===== 상수 정의 =====
        private const float TILE_SIZE = 1024f;
        
        // ===== 뷰 데이터 =====
        private BattleStage _battleStage;
        
        [Header("Tile Settings")]
        [Tooltip("인스펙터에서 설정할 RuleTile 배열")]
        public RuleTile[] ruleTiles; // RuleTile 배열로 변경
        
        [Header("Grid Size")]
        [Tooltip("타일맵의 X 크기")]
        public int gridSizeX = 50;
        [Tooltip("타일맵의 Y 크기")]
        public int gridSizeY = 50;
        
        // ===== 타일맵 시스템 =====
        private Tilemap tilemap;
        private Grid grid;
        
        // ===== 물리적 경계 콜리전 =====
        private TileBoundaryCollider boundaryCollider;
        
        
        // ===== 카메라 관련 =====
        private CameraController _cameraController;
        private Pawn _mainCharacter;
        [Header("Camera Settings")]
        public Vector3 cameraOffset = new Vector3(0, 0, -7);
        public float cameraFollowDistance = 10f;
        public Vector3 cameraDamping = new Vector3(1f, 1f, 1f);
        public float cameraFieldOfView = 60f;
        
        /// <summary>
        /// BattleStage 데이터에 대한 접근자
        /// 설정 시 양방향 연결을 자동으로 구성합니다.
        /// </summary>
        public BattleStage BattleStage
        {
            set
            {
                _battleStage = value;
                _battleStage.View = this;
                
                // TODO: 데이터 UI 동기화 로직 구현 필요
                // CreateTilemap();
                CreateBattleCamera();
            }

            get
            {
                return _battleStage;
            }
        }

        /// <summary>
        /// 지정된 중심과 크기로 랜덤 타일맵을 채웁니다.
        /// 각 타일은 랜덤하게 선택되고 회전됩니다.
        /// </summary>
        private void FillTilesWithRandomPattern(Vector3Int center, int width, int height)
        {
            if (ruleTiles == null || ruleTiles.Length == 0) 
            {
                Debug.LogWarning("[BATTLE_STAGE_VIEW] RuleTile이 설정되지 않았습니다.");
                return;
            }
            
            tilemap.ClearAllTiles();
            int halfWidth = width / 2;
            int halfHeight = height / 2;
            
            for (int y = center.y - halfHeight; y <= center.y + halfHeight; y++)
            {
                for (int x = center.x - halfWidth; x <= center.x + halfWidth; x++)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    
                    // 랜덤하게 RuleTile 선택
                    int randomIndex = UnityEngine.Random.Range(0, ruleTiles.Length);
                    RuleTile selectedTile = ruleTiles[randomIndex];
                    
                    // 랜덤 회전 (0, 90, 180, 270도)
                    int rotationIndex = UnityEngine.Random.Range(0, 4);
                    Matrix4x4 transformMatrix = Matrix4x4.TRS(
                        Vector3.zero, 
                        Quaternion.Euler(0, 0, rotationIndex * 90), 
                        Vector3.one
                    );
                    
                    tilemap.SetTile(tilePos, selectedTile);
                    tilemap.SetTransformMatrix(tilePos, transformMatrix);
                }
            }
        }

        // 게임 시작 시 호출
        private void CreateTilemap()
        {
            GameObject tilemapGO = new GameObject("StaticTilemap");
            tilemapGO.transform.SetParent(transform);
            tilemap = tilemapGO.AddComponent<Tilemap>();

            TilemapRenderer renderer = tilemapGO.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = -1;

            grid = tilemapGO.AddComponent<Grid>();
            // 타일 PNG 크기(1024x1024)를 그대로 사용
            grid.cellSize = new Vector3(TILE_SIZE, TILE_SIZE, 1);
            grid.cellGap = Vector3.zero;
            grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

            // 초기 타일맵 생성
            Vector3Int initialCenter = Vector3Int.zero;
            FillTilesWithRandomPattern(initialCenter, gridSizeX, gridSizeY);

            // 물리적 경계 콜리전 게임오브젝트 생성
            GameObject boundaryGO = new GameObject("TileBoundaryCollider");
            boundaryGO.transform.SetParent(transform);
            boundaryCollider = boundaryGO.AddComponent<TileBoundaryCollider>();
            boundaryCollider.Initialize(this);           

            Debug.Log($"[BATTLE_STAGE_VIEW] 정적 타일맵 생성 완료 (크기: {gridSizeX}x{gridSizeY})");
        }
        
        /// <summary>
        /// 전투용 카메라를 생성하고 설정합니다.
        /// </summary>
        private void CreateBattleCamera()
        {
            // 기존 카메라가 있다면 제거
            if (_cameraController != null)
            {
                DestroyImmediate(_cameraController.gameObject);
            }
            
            // 새로운 카메라 게임오브젝트 생성
            GameObject cameraGO = new GameObject("BattleStageCamera");
            cameraGO.transform.SetParent(transform);
            cameraGO.transform.position = new Vector3(0, 0, -5.5f);
            
            // CameraController 컴포넌트 추가
            _cameraController = cameraGO.AddComponent<CameraController>();
            _cameraController.Initialize();
            
            // 카메라 설정
            _cameraController.SetFollowOffset(cameraOffset);
            _cameraController.fieldOfView = cameraFieldOfView;
            _cameraController.UpdateLensSettings();
            
            Debug.Log("전투 카메라가 생성되었습니다.");
        }
        
        /// <summary>
        /// 메인 캐릭터를 설정하고 카메라가 따라다니도록 합니다.
        /// </summary>
        public void SetMainCharacter()
        {
            if (_battleStage?.mainCharacter != null && _cameraController != null)
            {
                _mainCharacter = _battleStage.mainCharacter;
                
                // 카메라가 캐릭터를 따라다니도록 설정
                _cameraController.SetTarget(_mainCharacter.transform);
                _cameraController.SetPriority(10);
                
                Debug.Log($"카메라가 {_mainCharacter.name}를 팔로우하도록 설정되었습니다.");
            }
        }
        
        /// <summary>
        /// 배틀 스테이지 업데이트
        /// </summary>
        private void Update()
        {
            _battleStage.Update();
        }
           
        /// <summary>
        /// 타일맵을 다시 생성합니다 (디버깅용)
        /// </summary>
        [ContextMenu("Regenerate Tilemap")]
        public void RegenerateTilemap()
        {
            if (tilemap != null)
            {
                Vector3Int center = Vector3Int.zero;
                FillTilesWithRandomPattern(center, gridSizeX, gridSizeY);
                Debug.Log("[BATTLE_STAGE_VIEW] 타일맵이 재생성되었습니다.");
            }
        }
        
        /// <summary>
        /// 현재 타일맵의 크기를 반환합니다.
        /// </summary>
        public Vector2Int GetTilemapSize()
        {
            return new Vector2Int(gridSizeX, gridSizeY);
        }
    }
} 