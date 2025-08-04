using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GamePlayer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace OutGame{
    public class RelicSelectedListView : MonoBehaviour
    {
        [Header("UI References")]
        public Transform relicListContainer; // 선택된 유물 리스트를 담을 컨테이너
        public GameObject selectedRelicItemPrefab; // 선택된 유물 UI 프리팹

        [Header("Selected Relics")]
        public List<AchievementData> selectedRelics = new List<AchievementData>(3); // 최대 3개 선택 가능
        
        [Header("Player Reference")]
        public Player player;
        
        private List<GameObject> relicItems = new List<GameObject>();
        private List<RelicView> relicViews = new List<RelicView>(3); // 0, 1, 2 인덱스용

        public void Awake()
        {           
            // 컨테이너가 할당되지 않았으면 자동으로 찾기
            if (relicListContainer == null)
            {
                relicListContainer = transform;
            }
            
            // 초기 UI 슬롯 생성 (3개)
            CreateInitialSlots();
        }
        
        // Addressable을 사용한 스프라이트 로드 메서드
        private void LoadSpriteFromAddressable(string addressableKey, System.Action<Sprite> onComplete)
        {
            addressableKey = "Assets/Trieyes/Addressable/Icons/Relics/" + addressableKey + ".png";

            if (string.IsNullOrEmpty(addressableKey))
            {
                Debug.LogWarning("AddressableKey가 비어있습니다.");
                onComplete?.Invoke(null);
                return;
            }
            
            Addressables.LoadAssetAsync<Sprite>(addressableKey).Completed += (AsyncOperationHandle<Sprite> handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    onComplete?.Invoke(handle.Result);
                }
                else
                {
                    Debug.LogWarning($"Addressable에서 스프라이트를 로드할 수 없습니다: {addressableKey}");
                    onComplete?.Invoke(null);
                }
            };
        }
        
        // 초기 UI 슬롯 생성 (0, 1, 2 인덱스)
        private void CreateInitialSlots()
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject relicItem = Instantiate(selectedRelicItemPrefab, relicListContainer);
                RelicView relicView = relicItem.GetComponent<RelicView>();

                relicView.Init();
                
                if (relicView != null)
                {
                    relicViews.Add(relicView);
                    relicItems.Add(relicItem);
                    
                    // 각 슬롯에 제거 버튼 이벤트 설정
                    SetupRemoveButton(relicView, i);
                }
            }
        }
        
        // 제거 버튼 설정
        private void SetupRemoveButton(RelicView relicView, int slotIndex)
        {
            if (relicView.relicSelectButton != null)
            {
                var button = relicView.relicSelectButton.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OnRemoveRelicClicked(slotIndex));
                }
            }
        }
        
        // UI 업데이트
        private void UpdateRelicUI()
        {
            // 모든 슬롯 초기화
            for (int i = 0; i < 3; i++)
            {
                if (i < selectedRelics.Count)
                {
                    // 선택된 유물이 있는 경우
                    SetSelectedRelic(relicViews[i], selectedRelics[i], i);
                }
                else
                {
                    // 빈 슬롯
                    SetEmptySlot(relicViews[i], i);
                }
            }
        }
        
        // 선택된 유물 설정
        private void SetSelectedRelic(RelicView relicView, AchievementData relic, int slotIndex)
        {
            relicView.relic = relic;

            // 1. relicIcon 설정 - 클릭 시 설명 표시
            if (relicView.relicIcon != null)
            {
                var relicIconButton = relicView.relicIcon.GetComponent<Button>();
                if (relicIconButton != null)
                {
                    relicIconButton.onClick.RemoveAllListeners();
                    relicIconButton.onClick.AddListener(() => OnRelicIconClicked(relicView));
                }

                // 아이콘 이미지 설정 - Addressable 사용
                var relicIconImage = relicView.relicIcon.GetComponent<Image>();
                if (relicIconImage != null)
                {
                    // AddressableKey가 있으면 Addressable 사용, 없으면 기존 방식 사용
                    if (!string.IsNullOrEmpty(relic.AddressableKey))
                    {
                        LoadSpriteFromAddressable(relic.AddressableKey, (sprite) =>
                        {
                            if (sprite != null)
                            {
                                relicIconImage.sprite = sprite;
                            }
                            else
                            {
                                Debug.LogWarning($"Addressable에서 유물 아이콘을 로드할 수 없습니다: {relic.achievementName} (Key: {relic.AddressableKey})");
                            }
                        });
                    }
                }
            }
            
            // 2. relicName 설정
            if (relicView.relicName != null)
            {
                var relicNameComponent = relicView.relicName.GetComponent<TextMeshProUGUI>();
                if (relicNameComponent != null)
                {
                    relicNameComponent.text = relic.achievementName;
                }
            }
            
            // 3. button 설정 - 유물 제거 버튼
            if (relicView.relicSelectButton != null)
            {
                var button = relicView.relicSelectButton.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = true;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OnRemoveRelicClicked(slotIndex));
                }

                // Text는 버튼 컴포넌트 자식의 자식에 있음.
                var buttonText = relicView.relicSelectButton.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "REMOVE";
                }
            }

            relicView.relicIcon.SetActive(true);
            relicView.relicSelectButton.SetActive(true);
        }
        
        // 빈 슬롯 설정
        private void SetEmptySlot(RelicView relicView, int slotIndex)
        {
            relicView.relic = null;
            relicView.Init();
        }
        
        // 유물 추가 (RelicListView에서 호출)
        public bool AddSelectedRelic(AchievementData relic)
        {
            if (selectedRelics.Count >= 3)
            {
                Debug.LogWarning("이미 최대 3개의 유물이 선택되어 있습니다!");
                return false;
            }
            
            // 중복 체크
            if (selectedRelics.Any(r => r.achievementID == relic.achievementID))
            {
                Debug.LogWarning("이미 선택된 유물입니다!");
                return false;
            }
            
            // 선택된 유물 목록에 추가
            selectedRelics.Add(relic);
            
            // player의 selectedRelicIds에도 추가
            if (player.selectedRelicIds == null)
            {
                player.selectedRelicIds = new List<int>();
            }
            player.selectedRelicIds.Add(relic.unlockElementID);
            
            // UI 업데이트
            UpdateRelicUI();
            
            Debug.Log($"유물 추가됨: {relic.achievementName} (총 {selectedRelics.Count}개)");
            return true;
        }
        
        // 유물 제거 (슬롯 인덱스로)
        public void OnRemoveRelicClicked(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= selectedRelics.Count)
            {
                Debug.LogWarning($"잘못된 슬롯 인덱스: {slotIndex}");
                return;
            }
            
            AchievementData removedRelic = selectedRelics[slotIndex];
            
            // 선택된 유물 목록에서 제거
            selectedRelics.RemoveAt(slotIndex);
            
            // player의 selectedRelicIds에서도 제거
            if (player.selectedRelicIds != null)
            {
                player.selectedRelicIds.Remove(removedRelic.unlockElementID);
            }
            
            // UI 업데이트
            UpdateRelicUI();
            
            Debug.Log($"유물 제거됨: {removedRelic.achievementName} (슬롯 {slotIndex})");
        }
        
        // relicIcon 클릭 이벤트 - 유물 설명 표시
        public void OnRelicIconClicked(RelicView relicView)
        {
            if (relicView.relic != null)
            {
                Debug.Log($"선택된 유물 아이콘 클릭: {relicView.relic.achievementName}");
                Debug.Log($"유물 설명: {relicView.relic.achievementDescription}");
                
                // 여기에 유물 설명을 표시하는 UI 로직 추가
                ShowRelicDescription(relicView.relic);
            }
        }
        
        // 유물 설명 표시 메서드
        private void ShowRelicDescription(AchievementData relic)
        {
            Debug.Log($"=== 선택된 유물 정보 ===");
            Debug.Log($"이름: {relic.achievementName}");
            Debug.Log($"설명: {relic.achievementDescription}");
            Debug.Log($"진행도: {relic.achievementProgressCurrent}/{relic.achievementProgressMax}");
            Debug.Log($"해금 여부: {(relic.IsUnlocked ? "해금됨" : "잠금됨")}");
        }
        
        // 선택된 유물 수 가져오기
        public int GetSelectedRelicCount()
        {
            return selectedRelics.Count;
        }
        
        // 최대 선택 가능한 유물 수
        public int GetMaxRelicCount()
        {
            return 3;
        }
        
        // 선택 가능한 슬롯이 있는지 확인
        public bool HasAvailableSlot()
        {
            return selectedRelics.Count < 3;
        }
        
        // 특정 유물이 선택되었는지 확인
        public bool IsRelicSelected(int relicId)
        {
            return selectedRelics.Any(relic => relic.achievementID == relicId);
        }
        
        // 선택된 유물 정보 가져오기
        public AchievementData GetSelectedRelicById(int relicId)
        {
            return selectedRelics.FirstOrDefault(relic => relic.achievementID == relicId);
        }
        
        // 모든 선택된 유물 제거
        public void ClearAllSelectedRelics()
        {
            selectedRelics.Clear();
            if (player.selectedRelicIds != null)
            {
                player.selectedRelicIds.Clear();
            }
            UpdateRelicUI();
            Debug.Log("모든 선택된 유물이 제거되었습니다.");
        }
    }
}