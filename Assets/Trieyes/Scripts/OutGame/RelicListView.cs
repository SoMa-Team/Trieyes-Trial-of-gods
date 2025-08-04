using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GamePlayer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace OutGame{
    public class RelicListView : MonoBehaviour
    {
        [Header("UI References")]
        public Transform relicListContainer; // 유물 리스트를 담을 컨테이너
        public GameObject unlockedRelicItemPrefab; // 해금된 유물 UI 프리팹
        public GameObject lockedRelicItemPrefab; // 잠금된 유물 UI 프리팹
        
        [Header("Player Reference")]
        public Player player;
        
        [Header("Relic Data")]
        public List<AchievementData> unlockedRelics; // 해금된 유물 목록
        
        [Header("Selected Relic List Reference")]
        public GameObject selectedRelicListView; // 선택된 유물 리스트 뷰
        
        private List<GameObject> relicItems = new List<GameObject>();

        public void Awake()
        {           
            // 컨테이너가 할당되지 않았으면 자동으로 찾기
            if (relicListContainer == null)
            {
                relicListContainer = transform;
            }
        }
        
        public void Start()
        {
            // 유물 데이터 로드 및 UI 생성
            LoadAndDisplayRelics();
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
        
        // 기존 Resources 로드 메서드 (폴백용)
        private Sprite LoadSpriteFromPath(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath))
            {
                Debug.LogWarning("아이콘 경로가 비어있습니다.");
                return null;
            }
            
            // Resources 폴더 기준으로 경로 변환
            string resourcePath = iconPath.Replace("Assets/", "").Replace(".png", "").Replace(".jpg", "");
            
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null)
            {
                Debug.LogWarning($"스프라이트를 찾을 수 없습니다: {resourcePath}");
                return null;
            }
            
            return sprite;
        }
        
        // 유물 데이터 로드 및 UI 표시
        public void LoadAndDisplayRelics()
        {
            if (player?.achievement == null)
            {
                Debug.LogError("Player 또는 Achievement가 null입니다!");
                return;
            }
            
            // 기존 UI 아이템들 제거
            ClearRelicItems();
            
            // player의 Achievement에서 해금한 유물 리스트를 가져온다.
            unlockedRelics = player.achievement.achievementDataList;

            foreach (var relic in unlockedRelics)
            {
                CreateRelicItem(relic.achievementID);
            }
        }
        
        // 유물 UI 아이템 생성 (해금/잠금 구분)
        private void CreateRelicItem(int relicId)
        {
            // 해당 유물이 해금되었는지 확인
            bool isUnlocked = player.achievement.achievementDictionary[relicId].IsUnlocked;
            
            // 적절한 프리팹 선택
            GameObject prefabToUse = isUnlocked ? unlockedRelicItemPrefab : lockedRelicItemPrefab;
            
            // 프리팹으로 유물 아이템 생성
            GameObject relicItem = Instantiate(prefabToUse, relicListContainer);
            relicItems.Add(relicItem);
            
            // 유물 정보 설정
            if (isUnlocked)
            {
                AchievementData unlockedRelic = unlockedRelics.FirstOrDefault(relic => relic.achievementID == relicId);
                RelicView relicView = relicItem.GetComponent<RelicView>();
                SetUnlockedRelic(relicView, unlockedRelic);
            }
            else
            {
                RelicView relicView = relicItem.GetComponent<RelicView>();
                SetLockedRelic(relicView, unlockedRelics.FirstOrDefault(relic => relic.achievementID == relicId));
            }
        }

        
        public void SetUnlockedRelic(RelicView relicView, AchievementData relic)
        {
            relicView.relic = relic;

            // 1. relicIcon 설정 - 클릭 시 설명 표시
            if (relicView.relicIcon != null)
            {
                var relicIconButton = relicView.relicIcon.GetComponent<Button>();
                if (relicIconButton != null)
                {
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
            
            // 2. relicName 직접 설정
            if (relicView.relicName != null)
            {
                var relicNameComponent = relicView.relicName.GetComponent<TextMeshProUGUI>();
                if (relicNameComponent != null)
                {
                    relicNameComponent.text = relic.achievementName;
                }
            }
            
            // 3. button 설정 - 유물 선택 버튼
            if (relicView.relicSelectButton != null)
            {
                var button = relicView.relicSelectButton.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnUnlockedRelicItemClicked(relic.achievementID));
                }
            }
        }

        public void SetLockedRelic(RelicView relicView, AchievementData relic)
        {
            relicView.relic = null;

            if (relicView.relicIcon != null)
            {
                var relicIconButton = relicView.relicIcon.GetComponent<Button>();
                if (relicIconButton != null)
                {
                    relicIconButton.onClick.RemoveAllListeners();
                }
            }

            if (relicView.relicName != null)
            {
                var relicNameComponent = relicView.relicName.GetComponent<TextMeshProUGUI>();
                if (relicNameComponent != null)
                {
                    relicNameComponent.text = "???";
                }
            }

            if (relicView.relicSelectButton != null)
            {
                var button = relicView.relicSelectButton.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = false; // 버튼 비활성화
                    button.onClick.RemoveAllListeners();
                }
            }
        }

        public void OnRelicIconClicked(RelicView relicView)
        {
            Debug.Log($"유물 아이콘 클릭: {relicView.relic.achievementName}");
        }
        
        // 해금된 유물 아이템 클릭 이벤트
        public void OnUnlockedRelicItemClicked(int relicId)
        {
            Debug.Log($"해금된 유물 선택 버튼 클릭: {relicId}");

            // RelicSelectedListView에 유물 추가
            if (selectedRelicListView != null)
            {
                var selectedRelicListView = this.selectedRelicListView.GetComponent<RelicSelectedListView>();
                AchievementData relicData = player.achievement.GetAchievementData(relicId);
                if (relicData != null)
                {
                    bool added = selectedRelicListView.AddSelectedRelic(relicData);
                    if (added)
                    {
                        Debug.Log($"유물이 선택 목록에 추가되었습니다: {relicData.achievementName}");
                    }
                    else
                    {
                        Debug.LogWarning("유물을 선택 목록에 추가할 수 없습니다.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("RelicSelectedListView가 할당되지 않았습니다!");
            }
        }

        // 기존 UI 아이템들 제거
        private void ClearRelicItems()
        {
            foreach (var item in relicItems)
            {
                if (item != null)
                {
                    DestroyImmediate(item);
                }
            }
            relicItems.Clear();
        }
        
        // 유물 리스트 새로고침
        public void RefreshRelicList()
        {
            LoadAndDisplayRelics();
        }
        
        // 특정 유물이 해금되었는지 확인
        public bool IsRelicUnlocked(int relicId)
        {
            return unlockedRelics.Any(relic => relic.achievementID == relicId);
        }
        
        // 해금된 유물 정보 가져오기
        public AchievementData GetUnlockedRelicById(int relicId)
        {
            return unlockedRelics.FirstOrDefault(relic => relic.achievementID == relicId);
        }
        
        // 해금된 유물 수 가져오기
        public int GetUnlockedRelicCount()
        {
            return unlockedRelics.Count;
        }
    }
}