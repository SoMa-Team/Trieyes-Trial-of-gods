using UnityEngine;

namespace CombatSystem
{
    public class Spawner : MonoBehaviour
    {
        /// <summary>
        /// 이 스폰 지점의 월드 위치를 반환합니다.
        /// </summary>
        /// <returns>스폰 위치 (Vector3)</returns>
        public Vector3 GetSpawnPosition()
        {
            return transform.position;
        }

        // 필요하다면 특정 스폰 영역(예: 반경, 박스)을 정의하거나
        // 시각화를 위한 Gizmo 등을 여기에 추가할 수 있습니다.
    }
} 