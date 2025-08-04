using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GamePlayer
{
    // 직렬화 가능한 업적 데이터
    [System.Serializable]
    public class SerializableAchievementData
    {
        public int id;
        public AchievementData data;
    }

    // 모든 업적 데이터를 관리하는 ScriptableObject 클래스
    [CreateAssetMenu(fileName = "AchievementDatabase", menuName = "Trieyes/Achievement Database")]
    public class AchievementDatabaseSO : ScriptableObject
    {
        [Header("업적 데이터 리스트 (직렬화용)")]
        public List<SerializableAchievementData> achievementDataList = new List<SerializableAchievementData>();
        
        [Header("업적 해금 상태 리스트 (직렬화용)")]
        public List<int> unlockedAchievementIds = new List<int>();

        // 런타임용 딕셔너리 (직렬화되지 않음)
        private Dictionary<int, AchievementData> _achievementDictionary;
        private Dictionary<int, bool> _achievementUnlockStatus;

        // 딕셔너리 프로퍼티 (런타임에서 사용)
        public Dictionary<int, AchievementData> achievementDictionary
        {
            get
            {
                if (_achievementDictionary == null)
                {
                    InitializeDictionaries();
                }
                return _achievementDictionary;
            }
        }

        public Dictionary<int, bool> achievementUnlockStatus
        {
            get
            {
                if (_achievementUnlockStatus == null)
                {
                    InitializeDictionaries();
                }
                return _achievementUnlockStatus;
            }
        }

        // 딕셔너리 초기화
        private void InitializeDictionaries()
        {
            _achievementDictionary = new Dictionary<int, AchievementData>();
            _achievementUnlockStatus = new Dictionary<int, bool>();

            // 리스트에서 딕셔너리로 변환
            foreach (var item in achievementDataList)
            {
                if (item.data != null)
                {
                    _achievementDictionary[item.id] = item.data;
                    _achievementUnlockStatus[item.id] = unlockedAchievementIds.Contains(item.id);
                }
            }
        }

        // 딕셔너리를 리스트로 변환 (저장용)
        public void ConvertDictionariesToList()
        {
            achievementDataList.Clear();
            unlockedAchievementIds.Clear();

            foreach (var kvp in _achievementDictionary)
            {
                achievementDataList.Add(new SerializableAchievementData
                {
                    id = kvp.Key,
                    data = kvp.Value
                });
            }

            foreach (var kvp in _achievementUnlockStatus)
            {
                if (kvp.Value)
                {
                    unlockedAchievementIds.Add(kvp.Key);
                }
            }
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

        // 모든 업적 목록 가져오기
        public List<AchievementData> GetAllAchievements()
        {
            return achievementDictionary.Values.ToList();
        }

        // 해금된 업적 목록 가져오기
        public List<AchievementData> GetUnlockedAchievements()
        {
            return achievementDictionary.Values.Where(a => a.IsUnlocked).ToList();
        }

        // 해금되지 않은 업적 목록 가져오기
        public List<AchievementData> GetLockedAchievements()
        {
            return achievementDictionary.Values.Where(a => !a.IsUnlocked).ToList();
        }
    }
} 