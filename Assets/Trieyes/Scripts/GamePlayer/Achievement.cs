using System.Collections.Generic;
using UnityEngine;
using RelicSystem;

namespace GamePlayer
{
    public enum AchievementType
    {
        None = 0,
        CharacterUnlock,
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
        
        // 업적 해금 상태 딕셔너리 (ID -> 해금여부)
        public Dictionary<int, bool> achievementUnlockStatus;
        
        public List<AchievementData> achievementDataList;
        
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
            achievementUnlockStatus = new Dictionary<int, bool>();
            achievementDataList = new List<AchievementData>();
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
            achievementUnlockStatus = new Dictionary<int, bool>(database.achievementUnlockStatus);
            achievementDataList = new List<AchievementData>(database.achievementDictionary.Values);
            
            Debug.Log($"ScriptableObject에서 업적 데이터 로드 완료: {achievementDictionary.Count}개");
        }
        
        // 특정 업적의 해금 상태 확인
        public bool IsAchievementUnlocked(int achievementId)
        {
            return achievementUnlockStatus.ContainsKey(achievementId) && achievementUnlockStatus[achievementId];
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
                achievementUnlockStatus[achievementId] = achievement.IsUnlocked;
                
                Debug.Log($"업적 진행도 업데이트: {achievement.achievementName} - {achievement.achievementProgressCurrent}/{achievement.achievementProgressMax}");
            }
        }
        
        // 모든 업적 해금 상태 확인
        public void CheckAllAchievements()
        {
            foreach (var kvp in achievementDictionary)
            {
                int id = kvp.Key;
                var achievement = kvp.Value;
                bool wasUnlocked = achievementUnlockStatus[id];
                bool isNowUnlocked = achievement.IsUnlocked;
                
                if (!wasUnlocked && isNowUnlocked)
                {
                    Debug.Log($"새로운 업적 해금: {achievement.achievementName}!");
                    achievementUnlockStatus[id] = true;
                }
            }
        }
        
        // 해금된 유물 목록 가져오기
        public List<AchievementData> GetUnlockedRelics()
        {
            List<AchievementData> unlockedRelics = new List<AchievementData>();
            
            foreach (var kvp in achievementDictionary)
            {
                var achievement = kvp.Value;
                
                // RelicUnlock 타입이고 해금된 업적인 경우
                if (achievement.achievementType == AchievementType.RelicUnlock && achievement.IsUnlocked)
                {
                    unlockedRelics.Add(achievement);
                }
            }
            
            // ID 오름차순으로 정렬
            unlockedRelics.Sort((a, b) => a.achievementID.CompareTo(b.achievementID));
            
            Debug.Log($"총 {unlockedRelics.Count}개의 해금된 유물을 찾았습니다.");
            return unlockedRelics;
        }
    }
} 