using UnityEngine;

namespace CharacterSystem
{
    /// <summary>
    /// 타겟을 따라가는 카메라 컴포넌트
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        // ===== [필드] =====
        /// <summary>
        /// 따라갈 대상(캐릭터)
        /// </summary>
        public Transform target;
        
        /// <summary>
        /// 카메라와의 거리(기본값: z축 -10)
        /// </summary>
        public Vector3 offset = new Vector3(0, 0, -10);

        // ===== [Unity 생명주기] =====
        /// <summary>
        /// 모든 Update 함수가 호출된 후 호출됩니다.
        /// </summary>
        void LateUpdate()
        {
            if (target == null) return;
            transform.position = target.position + offset;
        }
    }
}