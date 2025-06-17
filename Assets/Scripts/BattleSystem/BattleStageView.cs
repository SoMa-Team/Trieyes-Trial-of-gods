using UnityEngine;

namespace CombatSystem
{
    public class BattleStageView : MonoBehaviour
    {
        // 맵 디자인 프리팹 설정 등의 역할을 할 필드 (예시)
        public GameObject mapDesignPrefab;

        // 전투 UI 요소들에 대한 참조 (예시)
        // public GameObject healthBarUI;
        // public GameObject abilityButtonUI;

        /// <summary>
        /// 맵 디자인 프리팹을 설정하고 초기화하는 메서드 (SceneFactory 등에서 호출될 수 있음)
        /// </summary>
        /// <param name="mapPrefab">설정할 맵 프리팹</param>
        public void SetMapDesign(GameObject mapPrefab)
        {
            mapDesignPrefab = mapPrefab;
            // 실제 맵 인스턴스화 및 설정 로직
            Debug.Log($"CombatStageView: Setting map design to {mapPrefab.name}");
        }

        /// <summary>
        /// 전투 UI를 업데이트하는 메서드 (예: 캐릭터 스탯 변경 시)
        /// </summary>
        public void UpdateCombatUI()
        {
            // UI 업데이트 로직
            // Debug.Log("CombatStageView: Updating combat UI.");
        }

        // 기타 Unity 전용 함수 (예: 애니메이션, 이펙트 등) 구현 가능
    }
} 