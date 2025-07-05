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
        // ===== 뷰 데이터 =====
        private BattleStage _battleStage;
        public GameObject spriteRectPrefabs;
        public TileBase ruleTile; // 인스펙터에서 사용할 타일 배열 할당
        public int Height { get; internal set; }
        public int Width { get; internal set; }
        
        // ===== 타일 풀링 시스템 =====
        private TilePoolingSystem tilePoolingSystem;
        private Tilemap tilemap;
        private Vector3Int lastTileCenter;
        private bool isTileSystemInitialized = false;
        
        // ===== 콜리전 기반 경계 시스템 =====
        private TileBoundaryCollider boundaryCollider;
        
        [Header("Tile Pooling Settings")]
        public float boundaryThreshold = 0.5f; // 경계 임계값 (80%)
        
        // ===== 카메라 관련 =====
        private Camera _battleCamera;
        private Pawn _mainCharacter;
        [Header("Camera Settings")]
        public float cameraFollowSpeed = 5f;
        public Vector3 cameraOffset = new Vector3(0, 0, -7);
        public float cameraSmoothTime = 0.1f;
        private Vector3 _cameraVelocity = Vector3.zero;
        
        [Header("Dynamic Camera Settings")]
        public float minCameraDistance = 5f;  // 최소 카메라 거리
        public float maxCameraDistance = 15f; // 최대 카메라 거리
        public float speedToDistanceMultiplier = 2f; // 속도에 따른 거리 배수
        private Vector3 _dynamicCameraOffset;
        
        /// <summary>
        /// BattleStage 데이터에 대한 접근자
        /// 설정 시 양방향 연결을 자동으로 구성합니다.
        /// </summary>
        public BattleStage BattleStage
        {
            set
            {
                Height = 30;
                Width = 30;
                _battleStage = value;
                _battleStage.View = this;
                
                // TODO: 데이터 UI 동기화 로직 구현 필요
                CreateSpriteRect();
                CreateBattleCamera();
            }

            get
            {
                return _battleStage;
            }
        }

        private void CreateSpriteRect()
        {
            // 1. 프리팹을 씬에 인스턴스화
            GameObject tilemapGO = Instantiate(spriteRectPrefabs);

            // 2. Tilemap 컴포넌트 가져오기
            tilemap = tilemapGO.GetComponentInChildren<Tilemap>();
            if (tilemap == null)
            {
                Debug.LogError("Tilemap 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = -1;
            }

            // 3. 타일 풀링 시스템 초기화
            InitializeTilePoolingSystem();
            
            // 4. 초기 타일 생성 (중앙 기준)
            Vector3Int initialCenter = Vector3Int.zero;
            tilePoolingSystem.CreateTilesInRange(initialCenter, Width, Height);
            lastTileCenter = initialCenter;
            
            Debug.Log($"[BATTLE_STAGE_VIEW] 초기 타일 생성 완료 (중심: {initialCenter}, 크기: {Width}x{Height})");
        }
        
        /// <summary>
        /// 타일 풀링 시스템과 콜리전 기반 경계 시스템을 초기화합니다.
        /// </summary>
        private void InitializeTilePoolingSystem()
        {
            // 타일 풀링 시스템 컴포넌트 추가
            tilePoolingSystem = gameObject.AddComponent<TilePoolingSystem>();
            tilePoolingSystem.boundaryThreshold = boundaryThreshold;
            
            // 타일 풀링 시스템 초기화
            tilePoolingSystem.Initialize(tilemap, ruleTile);
            isTileSystemInitialized = true;
            
            // 콜리전 기반 경계 시스템 초기화
            InitializeBoundaryCollider();
            
            Debug.Log("[BATTLE_STAGE_VIEW] 타일 풀링 시스템과 콜리전 기반 경계 시스템이 초기화되었습니다.");
        }
        
        /// <summary>
        /// 콜리전 기반 경계 시스템을 초기화합니다.
        /// </summary>
        private void InitializeBoundaryCollider()
        {
            // 경계 콜리전 게임오브젝트 생성
            GameObject boundaryGO = new GameObject("TileBoundaryCollider");
            boundaryGO.transform.SetParent(transform);
            
            // TileBoundaryCollider 컴포넌트 추가
            boundaryCollider = boundaryGO.AddComponent<TileBoundaryCollider>();
            boundaryCollider.boundaryThreshold = boundaryThreshold;
            boundaryCollider.tileWidth = Width;
            boundaryCollider.tileHeight = Height;
            
            // 콜리전 시스템 초기화
            boundaryCollider.Initialize(this);
            
            Debug.Log("[BATTLE_STAGE_VIEW] 콜리전 기반 경계 시스템이 초기화되었습니다.");
        }
        
        /// <summary>
        /// 전투용 카메라를 생성하고 설정합니다.
        /// </summary>
        private void CreateBattleCamera()
        {
            // 기존 카메라가 있다면 제거
            if (_battleCamera != null)
            {
                DestroyImmediate(_battleCamera.gameObject);
            }
            
            // 새로운 카메라 게임오브젝트 생성
            GameObject cameraGO = new GameObject("BattleStageCamera");
            cameraGO.transform.SetParent(transform);
            
            // 카메라 컴포넌트 추가
            _battleCamera = cameraGO.AddComponent<Camera>();
            _battleCamera.orthographic = true;
            _battleCamera.orthographicSize = 5f;
            _battleCamera.backgroundColor = Color.black;
            
            // 카메라를 메인 카메라로 설정
            _battleCamera.tag = "MainCamera";
            
            // 초기 위치 설정
            _battleCamera.transform.position = cameraOffset;
            
            Debug.Log("전투 카메라가 생성되었습니다.");
        }
        
        /// <summary>
        /// 메인 캐릭터를 설정하고 카메라가 따라다니도록 합니다.
        /// </summary>
        public void SetMainCharacter()
        {
            if (_battleStage?.mainCharacter != null && _battleCamera != null)
            {
                _mainCharacter = _battleStage.mainCharacter;
                
                // 동적 카메라 오프셋 초기화
                _dynamicCameraOffset = cameraOffset;
                
                // 카메라를 캐릭터 위치로 즉시 이동
                Vector3 targetPosition = _mainCharacter.transform.position + _dynamicCameraOffset;
                _battleCamera.transform.position = targetPosition;
                
                Debug.Log($"카메라가 {_mainCharacter.name}를 팔로우하도록 설정되었습니다.");
            }
        }
        
        /// <summary>
        /// 카메라가 메인 캐릭터를 부드럽게 따라다니도록 업데이트합니다.
        /// 캐릭터의 이동 속도에 따라 카메라 거리가 동적으로 조절됩니다.
        /// 경계 기반 타일 관리도 함께 수행합니다.
        /// </summary>
        private void Update()
        {
            if (_mainCharacter != null && _battleCamera != null)
            {                
                // 목표 위치 계산 (동적 오프셋 사용)
                Vector3 targetPosition = _mainCharacter.transform.position + _dynamicCameraOffset;
                
                // 부드러운 이동
                _battleCamera.transform.position = Vector3.SmoothDamp(
                    _battleCamera.transform.position, 
                    targetPosition, 
                    ref _cameraVelocity, 
                    cameraSmoothTime
                );
            }
        }
           
        /// <summary>
        /// 콜리전 기반 경계 시스템에서 호출되는 타일 업데이트 메서드입니다.
        /// </summary>
        /// <param name="newCenter">새로운 타일 중심</param>
        public void UpdateTilesAtBoundary(Vector3Int newCenter)
        {
            if (tilePoolingSystem == null) return;
            
            Debug.Log($"[BATTLE_STAGE_VIEW] 콜리전 기반 타일 업데이트 시작: {newCenter}");
            
            // 배치 처리를 위한 성능 최적화
            StartCoroutine(UpdateTilesBatch(newCenter));
        }
        
        /// <summary>
        /// 배치로 타일을 업데이트합니다. (성능 최적화)
        /// </summary>
        private System.Collections.IEnumerator UpdateTilesBatch(Vector3Int newCenter)
        {
            // 1. 범위 밖의 타일들을 제거하고 풀로 반환 (O(log n))
            int removedCount = tilePoolingSystem.RemoveTilesOutsideRange(newCenter, Width, Height);
            
            // 프레임 분할을 위한 yield
            yield return null;
            
            // 2. 새로운 범위의 타일들을 생성 (O(n) 최적화)
            int createdCount = tilePoolingSystem.CreateTilesInRange(newCenter, Width, Height);
            
            // 3. 마지막 타일 중심 업데이트
            lastTileCenter = newCenter;
            
            Debug.Log($"[BATTLE_STAGE_VIEW] 배치 타일 업데이트 완료 - 제거: {removedCount}개, 생성: {createdCount}개");
            
            // 성능 통계 출력
            if (tilePoolingSystem != null)
            {
                tilePoolingSystem.LogPerformanceStats();
            }
        }
        
        /// <summary>
        /// 타일 풀링 시스템의 상태를 로그로 출력합니다.
        /// </summary>
        public void LogTileSystemStatus()
        {
            if (tilePoolingSystem != null)
            {
                tilePoolingSystem.LogStatus();
                Debug.Log($"[BATTLE_STAGE_VIEW] 현재 타일 중심: {lastTileCenter}");
            }
            else
            {
                Debug.Log("[BATTLE_STAGE_VIEW] 타일 풀링 시스템이 초기화되지 않았습니다.");
            }
        }
    }
} 