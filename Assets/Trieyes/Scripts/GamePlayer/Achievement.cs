using System.Collections.Generic;
using System.Linq;
using CardSystem;
using RelicSystem;
using UnityEngine;

namespace GamePlayer
{
    public enum AchievementType
    {
        None = 0,
        CharacterUnlock,
        SkillUnlock,
        RelicUnlock,
    }
    
    [System.Serializable]
    public class AchievementData
    {
        public AchievementType achievementType;
        public int achievementID;
        public string achievementName;
        public string achievementDescription;
        public int achievementProgressCurrent;
        public int achievementProgressMax;

        public int unlockElementID;

        public string AddressableKey;
        public Sprite achievementIcon;
        
        // 업적 해금 여부를 확인하는 프로퍼티
        public bool IsUnlocked => achievementProgressCurrent >= achievementProgressMax;
    }
    
    public class Achievement
    {
        // ===== [기능 1] 유저 정보 =====
        public string userName;
        public int userId;

        // 업적 데이터 딕셔너리 (ID -> AchievementData)
        public Dictionary<int, AchievementData> achievementDictionary;
        
        // 생성자에서 CSV 파일을 읽어서 초기화
        public Achievement()
        {
            InitializeAchievements();
        }
        
        // ScriptableObject로부터 초기화하는 생성자
        public Achievement(AchievementDatabaseSO database)
        {
            InitializeFromScriptableObject(database);
        }
        
        private void InitializeAchievements()
        {
            achievementDictionary = new Dictionary<int, AchievementData>();
        }
        
        private void InitializeFromScriptableObject(AchievementDatabaseSO database)
        {
            if (database == null)
            {
                Debug.LogError("AchievementDatabaseSO가 null입니다!");
                InitializeAchievements(); // 폴백으로 CSV 로드
                return;
            }
            
            achievementDictionary = new Dictionary<int, AchievementData>(database.achievementDictionary);
        }
        
        // 특정 업적 데이터 가져오기
        public AchievementData GetAchievementData(int achievementId)
        {
            return achievementDictionary.ContainsKey(achievementId) ? achievementDictionary[achievementId] : null;
        }
        
        // 업적 진행도 업데이트
        public void UpdateAchievementProgress(int achievementId, int newProgress)
        {
            if (achievementDictionary.ContainsKey(achievementId))
            {
                var achievement = achievementDictionary[achievementId];
                achievement.achievementProgressCurrent = Mathf.Min(newProgress, achievement.achievementProgressMax);
                
                Debug.Log($"업적 진행도 업데이트: {achievement.achievementName} - {achievement.achievementProgressCurrent}/{achievement.achievementProgressMax}");
            }
        }

        public List<int> GetAvailableRelicIDs()
        {
            // TODO : 업적 구현 시 수정 필요
            return RelicDataBase.GetAllRelicIDs().Where(x => x is >= 101 and <= 120).ToList();
        }
        
        public List<Card> GetAvailableCards()
        {
            // TODO : 카드 풀 가져오기
            return null;
        }
    }
} 
