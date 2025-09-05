// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using GamePlayer;
// using System.Collections.Generic;
// using System.Linq;

// namespace OutGame{
//     public class RelicFullListView : MonoBehaviour
//     {
//         [HideInInspector] public Transform relicListContainer; // 유물 리스트를 담을 컨테이너
        
//         [Header("UI References")]
//         public GameObject unlockedRelicItemPrefab; // 해금된 유물 UI 프리팹
//         public GameObject lockedRelicItemPrefab; // 잠금된 유물 UI 프리팹
        
//         [Header("Player Reference")]
//         public Player player;
        
//         [Header("Relic Data")]
//         public List<AchievementData> unlockedRelics; // 해금된 유물 목록
        
//         [Header("Selected Relic List Reference")]
//         public GameObject selectedRelicListView; // 선택된 유물 리스트 뷰
        
//         private List<GameObject> relicItems = new List<GameObject>();

//         public void Awake()
//         {           
//             // 컨테이너가 할당되지 않았으면 자동으로 찾기
//             if (relicListContainer == null)
//             {
//                 relicListContainer = transform;
//             }
//         }
        
//         public void Start()
//         {
//             // 유물 데이터 로드 및 UI 생성
//             LoadAndDisplayRelics();
//         }
        
//         // 유물 데이터 로드 및 UI 표시
//         public void LoadAndDisplayRelics()
//         {
//             if (player?.achievement == null)
//             {
//                 Debug.LogError("Player 또는 Achievement가 null입니다!");
//                 return;
//             }
            
//             // 기존 UI 아이템들 제거
//             ClearRelicItems();
            
//             // player의 Achievement에서 유물 리스트를 가져온다.
//             unlockedRelics = player.achievement.achievementDictionary.Values.Where(achievement => achievement.achievementType == AchievementType.RelicUnlock).ToList();

//             foreach (var relic in unlockedRelics)
//             {
//                 CreateRelicItem(relic.achievementID);
//             }
//         }
        
//         // 유물 UI 아이템 생성 (해금/잠금 구분)
//         private void CreateRelicItem(int relicId)
//         {
//             // 해당 유물이 해금되었는지 확인
//             bool isUnlocked = player.achievement.achievementDictionary[relicId].IsUnlocked;
            
//             // 적절한 프리팹 선택
//             GameObject prefabToUse = isUnlocked ? unlockedRelicItemPrefab : lockedRelicItemPrefab;
            
//             // 프리팹으로 유물 아이템 생성
//             GameObject relicItem = Instantiate(prefabToUse, relicListContainer);
//             relicItems.Add(relicItem);
            
//             // 유물 정보 설정
//             if (isUnlocked)
//             {
//                 AchievementData unlockedRelic = unlockedRelics.FirstOrDefault(relic => relic.achievementID == relicId);
//                 RelicView relicView = relicItem.GetComponent<RelicView>();
//                 SetUnlockedRelic(relicView, unlockedRelic);
//             }
//             else
//             {
//                 RelicView relicView = relicItem.GetComponent<RelicView>();
//                 SetLockedRelic(relicView, unlockedRelics.FirstOrDefault(relic => relic.achievementID == relicId));
//             }
//         }

        
//         public void SetUnlockedRelic(RelicView relicView, AchievementData relic)
//         {
//             relicView.relic = relic;

//             // 1. relicIcon 설정 - 클릭 시 설명 표시
//             if (relicView.relicIcon != null)
//             {
//                 var relicIconButton = relicView.relicIcon.GetComponent<Button>();
//                 if (relicIconButton != null)
//                 {
//                     relicIconButton.onClick.AddListener(() => OnRelicIconClicked(relicView));
//                 }
                
//                 // 아이콘 이미지 설정 - SO에 저장된 스프라이트 사용
//                 var relicIconImage = relicView.relicIcon.GetComponent<Image>();
//                 if (relicIconImage != null)
//                 {
//                     // SO에 저장된 스프라이트가 있으면 사용
//                     if (relic.achievementIcon != null)
//                     {
//                         relicIconImage.sprite = relic.achievementIcon;
//                     }
//                 }
//             }
            
//             // 2. relicName 직접 설정
//             if (relicView.relicName != null)
//             {
//                 var relicNameComponent = relicView.relicName.GetComponent<TextMeshProUGUI>();
//                 if (relicNameComponent != null)
//                 {
//                     relicNameComponent.text = relic.achievementName;
//                 }
//             }
            
//             // 3. button 설정 - 유물 선택 버튼
//             if (relicView.relicSelectButton != null)
//             {
//                 var button = relicView.relicSelectButton.GetComponent<Button>();
//                 if (button != null)
//                 {
//                     button.onClick.AddListener(() => OnUnlockedRelicItemClicked(relic.achievementID));
//                 }
//             }
//         }

//         public void SetLockedRelic(RelicView relicView, AchievementData relic)
//         {
//             relicView.relic = null;

//             if (relicView.relicIcon != null)
//             {
//                 var relicIconButton = relicView.relicIcon.GetComponent<Button>();
//                 if (relicIconButton != null)
//                 {
//                     relicIconButton.onClick.RemoveAllListeners();
//                 }
//             }

//             if (relicView.relicName != null)
//             {
//                 var relicNameComponent = relicView.relicName.GetComponent<TextMeshProUGUI>();
//                 if (relicNameComponent != null)
//                 {
//                     relicNameComponent.text = "???";
//                 }
//             }

//             if (relicView.relicSelectButton != null)
//             {
//                 var button = relicView.relicSelectButton.GetComponent<Button>();
//                 if (button != null)
//                 {
//                     button.interactable = false; // 버튼 비활성화
//                     button.onClick.RemoveAllListeners();
//                 }
//             }
//         }

//         // 해금된 유물 아이템 아이콘 클릭 이벤트
//         public void OnRelicIconClicked(RelicView relicView)
//         {
//             Debug.Log($"유물 아이콘 클릭: {relicView.relic.achievementName}");
//         }
        
//         // 해금된 유물 아이템 선택 버튼 클릭 이벤트
//         public void OnUnlockedRelicItemClicked(int relicId)
//         {
//             Debug.Log($"해금된 유물 선택 버튼 클릭: {relicId}");

//             // RelicSelectedListView에 유물 추가
//             if (selectedRelicListView != null)
//             {
//                 var selectedRelicListView = this.selectedRelicListView.GetComponent<RelicSelectedListView>();
//                 AchievementData relicData = player.achievement.GetAchievementData(relicId);
//                 if (relicData != null)
//                 {
//                     bool added = selectedRelicListView.AddSelectedRelic(relicData);
//                     if (added)
//                     {
//                         Debug.Log($"유물이 선택 목록에 추가되었습니다: {relicData.achievementName}");
//                     }
//                     else
//                     {
//                         Debug.LogWarning("유물을 선택 목록에 추가할 수 없습니다.");
//                     }
//                 }
//             }
//             else
//             {
//                 Debug.LogWarning("RelicSelectedListView가 할당되지 않았습니다!");
//             }
//         }

//         // 기존 UI 아이템들 제거
//         private void ClearRelicItems()
//         {
//             foreach (var item in relicItems)
//             {
//                 if (item != null)
//                 {
//                     DestroyImmediate(item);
//                 }
//             }
//             relicItems.Clear();
//         }
//     }
// }