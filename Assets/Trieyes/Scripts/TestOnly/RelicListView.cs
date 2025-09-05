using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GamePlayer;
using System.Collections.Generic;
using System.Linq;

namespace OutGame{
    public class RelicListView : ListView
    {
        public const int RELIC_COUNT = 3;
        
        public GameObject relicItemPrefab;
        public AchievementData selectedRelic;

        
        [Header("Player Reference")]
        public Player player;
        
        [Header("Relic Data")]
        public List<AchievementData> unlockedRelics; // 해금된 유물 목록

        public override void Activate()
        {
            if (relicItemPrefab != null)
            {
                // 해금된 유물 리스트 가져오기
                LoadUnlockedRelics();
                
                // 3개의 겹치지 않는 랜덤 유물 ID 선택
                List<int> selectedRelicIds = GetRandomRelicIds(RELIC_COUNT);
                
                for (int i = 0; i < RELIC_COUNT; i++)
                {
                    var obj = Instantiate(relicItemPrefab, transform);
                    var relicView = obj.GetComponent<RelicView>();

                    var relicSelectView = obj.GetComponent<RelicSelectView>();
                    
                    // 선택된 유물 ID로 유물 데이터 가져오기
                    int relicId = selectedRelicIds[i];
                    AchievementData relicData = GetRelicDataById(relicId);
                    
                    // RelicView에 유물 데이터 설정
                    if (relicData != null)
                    {
                        SetRelicData(relicView, relicData);
                    }
                    
                    // RelicSelectView에 유물 데이터 설정
                    if (relicSelectView != null)
                    {
                        relicSelectView.relicAchievementData = relicData;
                        relicSelectView.SetRelicListView(this);
                    }
                    
                    SetRelicListView(relicView);
                    obj.SetActive(true);
                }
            }
            selectedRelic = null;

            base.Activate();
        }

        public override void Deactivate()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            selectedRelic = null;
            base.Deactivate();
        }
        
        public void SetRelicListView(RelicView relicView)
        {
            // RelicSelectView 컴포넌트가 있다면 RelicListView 참조 설정
            var relicSelectView = relicView.GetComponent<RelicSelectView>();
            if (relicSelectView != null)
            {
                relicSelectView.SetRelicListView(this);
            }
        }
        
        public void ToGameStart()
        {
            Player.Instance.selectedRelic = selectedRelic;
            StartSceneManager.Instance.GameStart();
        }
        
        // 해금된 유물 리스트 로드
        private void LoadUnlockedRelics()
        {
            if (player?.achievement == null)
            {
                Debug.LogError("Player 또는 Achievement가 null입니다!");
                return;
            }
            
            // player의 Achievement에서 해금된 유물 리스트를 가져온다.
            unlockedRelics = player.achievement.achievementDictionary.Values
                .Where(achievement => achievement.achievementType == AchievementType.RelicUnlock && achievement.IsUnlocked)
                .ToList();
        }
        
        // 겹치지 않는 랜덤 유물 ID 리스트 생성
        private List<int> GetRandomRelicIds(int count)
        {
            if (unlockedRelics == null || unlockedRelics.Count == 0)
            {
                Debug.LogWarning("해금된 유물이 없습니다!");
                return new List<int>();
            }
            
            // 해금된 유물 ID 리스트 생성
            List<int> availableIds = unlockedRelics.Select(relic => relic.achievementID).ToList();
            
            // 리스트 셔플하고 첫번째 3개를 선택
            List<int> selectedIds = availableIds.OrderBy(x => UnityEngine.Random.value).Take(count).ToList();
            
            return selectedIds;
        }
        
        // ID로 유물 데이터 가져오기
        private AchievementData GetRelicDataById(int relicId)
        {
            return unlockedRelics?.FirstOrDefault(relic => relic.achievementID == relicId);
        }
        
        // RelicView에 유물 데이터 설정
        private void SetRelicData(RelicView relicView, AchievementData relicData)
        {
            if (relicView == null || relicData == null) return;
            
            relicView.relic = relicData;
            
            // 아이콘 설정
            if (relicView.relicIcon != null)
            {
                var relicIconImage = relicView.relicIcon.GetComponent<Image>();
                if (relicIconImage != null && relicData.achievementIcon != null)
                {
                    relicIconImage.sprite = relicData.achievementIcon;
                }
            }
            
            // 이름 설정
            if (relicView.relicName != null)
            {
                var relicNameComponent = relicView.relicName.GetComponent<TextMeshProUGUI>();
                if (relicNameComponent != null)
                {
                    relicNameComponent.text = relicData.achievementName;
                }
            }
        }
    }
}