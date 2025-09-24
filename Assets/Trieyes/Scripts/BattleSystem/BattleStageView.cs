using UnityEngine;
using CharacterSystem;
using System;

namespace BattleSystem
{
    /// <summary>
    /// 전투 스테이지의 뷰 컴포넌트
    /// BattleStage 데이터와 Unity GameObject를 연결하는 역할을 합니다.
    /// </summary>
    public abstract class BattleStageView : MonoBehaviour
    {
        // ===== 뷰 데이터 =====
        private BattleStage _battleStage;
        
        // ===== 카메라 관련 =====
        private CameraController _cameraController;
        private Pawn _mainCharacter;
        [Header("Camera Settings")]
        public Vector3 cameraOffset = new Vector3(0, 0, -7);
        public float cameraFollowDistance = 10f;
        public Vector3 cameraDamping = new Vector3(1f, 1f, 1f);
        public float cameraFieldOfView = 60f;

        [Header("Tiles")]
        [SerializeField] protected GameObject gridObject;
        private GameObject _gridObject;
        [SerializeField] protected GameObject backgroundObject;
        private GameObject _backgroundObject;
        
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
                CreateTilemap();
                CreateBattleCamera();
            }

            get
            {
                return _battleStage;
            }
        }

        private void CreateTilemap()
        {
            if (gridObject != null)
            {
                _gridObject = Instantiate(gridObject, new Vector3(0, 0, 0), Quaternion.identity);
            }
            if (backgroundObject != null)
            {
                _backgroundObject = Instantiate(backgroundObject, new Vector3(0, 0, 0), Quaternion.identity);
            }
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

        public void Update()
        {
            if(_battleStage == null || _battleStage.difficulty == null)
                return;
            
            _battleStage.Update();
        }

        public void Deactivate()
        {
            if (gridObject != null)
            {
                Destroy(_gridObject);
            }
            if (backgroundObject != null)
            {
                Destroy(_backgroundObject);
            }
        }
    }
} 