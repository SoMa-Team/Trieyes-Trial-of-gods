using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using CharacterSystem;

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
                Height = 100;
                Width = 100;
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
            // TODO : 테스트 이후 제거 필요.
            if (spriteRectPrefabs is null || ruleTile is null) return;
            // TODO END
            
            // 1. 프리팹을 씬에 인스턴스화
            GameObject tilemapGO = Instantiate(spriteRectPrefabs);

            // 2. Tilemap 컴포넌트 가져오기
            Tilemap tilemap = tilemapGO.GetComponentInChildren<Tilemap>();
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

            // 3. 원하는 위치에 타일 그리기 (예시: 전체를 같은 타일로 채우기)
            // -에서 시작해서 +방향으로 그리기
            for (int y = Height / 2; y >= -Height / 2; y--)
            {
                for (int x = -Width / 2; x < Width / 2; x++)
                {
                    // 예시: tiles[0]을 전체에 채움. 필요시 타입별로 분기
                    tilemap.SetTile(new Vector3Int(x, y, 0), ruleTile);

                    // Z-order -1
                    tilemap.SetTileFlags(new Vector3Int(x, y, 0), TileFlags.None);
                }
            }
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
        /// </summary>
        private void Update()
        {
            if (_mainCharacter != null && _battleCamera != null)
            {
                // 캐릭터의 이동 속도에 따른 동적 카메라 거리 계산
                UpdateDynamicCameraDistance();
                
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
        /// 캐릭터의 이동 속도에 따라 카메라 거리를 동적으로 조절합니다.
        /// </summary>
        private void UpdateDynamicCameraDistance()
        {
            if (_mainCharacter == null) return;
            
            // 캐릭터의 이동 속도 가져오기
            float characterSpeed = _mainCharacter.moveSpeed;
            
            // 속도에 따른 거리 계산 (속도가 빠를수록 카메라가 멀어짐)
            float dynamicDistance = Mathf.Lerp(
                minCameraDistance, 
                maxCameraDistance, 
                (characterSpeed - 1f) / (10f - 1f) // 1~10 속도 범위를 0~1로 정규화
            );
            
            // 거리 제한 적용
            dynamicDistance = Mathf.Clamp(dynamicDistance, minCameraDistance, maxCameraDistance);
            
            // 동적 카메라 오프셋 업데이트 (Z축만 변경)
            _dynamicCameraOffset = new Vector3(
                cameraOffset.x,
                cameraOffset.y,
                -dynamicDistance
            );
        }
    }
} 