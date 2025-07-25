using UnityEngine;
using Unity.Cinemachine;

namespace CharacterSystem
{
    /// <summary>
    /// Cinemachine을 사용한 카메라 컨트롤러
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Cinemachine Camera Settings")]
        public CinemachineCamera virtualCamera;
        
        [Header("Camera Follow Settings")]
        public float followDistance = 10f;
        public Vector3 followOffset = Vector3.zero;
        
        [Header("Camera Lens Settings")]
        public float fieldOfView = 60f;
        public float nearClipPlane = 0.1f;
        public float farClipPlane = 5000f;
        
        /// <summary>
        /// 카메라 컨트롤러 초기화
        /// </summary>
        public void Initialize()
        {
            if (virtualCamera == null)
            {
                virtualCamera = GetComponent<CinemachineCamera>();
                if (virtualCamera == null)
                {
                    virtualCamera = gameObject.AddComponent<CinemachineCamera>();
                }
            }
            
            SetupCameraComponents();
        }
        
        /// <summary>
        /// 카메라 컴포넌트들을 설정합니다
        /// </summary>
        private void SetupCameraComponents()
        {
            // Lens 설정
            var lensSettings = virtualCamera.Lens;
            lensSettings.FieldOfView = fieldOfView;
            lensSettings.NearClipPlane = nearClipPlane;
            lensSettings.FarClipPlane = farClipPlane;
            virtualCamera.Lens = lensSettings;
        }
        
        /// <summary>
        /// 타겟을 설정하고 카메라가 따라다니도록 합니다
        /// </summary>
        /// <param name="target">따라갈 타겟 Transform</param>
        public void SetTarget(Transform target)
        {
            if (virtualCamera == null) return;
            
            virtualCamera.Target.TrackingTarget = target;
            virtualCamera.Target.LookAtTarget = target;
            
            // Position Control을 Follow로 설정
            var positionComponent = virtualCamera.GetComponent<CinemachineFollow>();
            if (positionComponent == null)
            {
                // CinemachineFollowComponent를 추가
                var followComponent = virtualCamera.gameObject.AddComponent<CinemachineFollow>();
                followComponent.FollowOffset = followOffset;
            }
        }
        
        /// <summary>
        /// 카메라 우선순위를 설정합니다
        /// </summary>
        /// <param name="priority">우선순위 값</param>
        public void SetPriority(int priority)
        {
            if (virtualCamera != null)
            {
                virtualCamera.Priority = priority;
            }
        }
        
        /// <summary>
        /// 카메라 오프셋을 설정합니다
        /// </summary>
        /// <param name="offset">새로운 오프셋</param>
        public void SetFollowOffset(Vector3 offset)
        {
            followOffset = offset;
        }
        
        /// <summary>
        /// 카메라 렌즈 설정을 업데이트합니다
        /// </summary>
        public void UpdateLensSettings()
        {
            if (virtualCamera != null)
            {
                var lensSettings = virtualCamera.Lens;
                lensSettings.FieldOfView = fieldOfView;
                lensSettings.NearClipPlane = nearClipPlane;
                lensSettings.FarClipPlane = farClipPlane;
                virtualCamera.Lens = lensSettings;
            }
        }
        
        /// <summary>
        /// 카메라가 활성 상태인지 확인합니다
        /// </summary>
        /// <returns>활성 상태 여부</returns>
        public bool IsLive()
        {
            return virtualCamera != null;
        }
    }
}
