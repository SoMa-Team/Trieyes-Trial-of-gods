using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace OutGame
{
    // 공통 인터페이스
    public interface IAchievementObject
    {
        int Id { get; }
        string Name { get; }
        string Description { get; }
        string UnlockConditionDescription { get; }
        bool IsUnlocked { get; set; }
        List<UnlockCondition> UnlockConditions { get; }
        List<UnlockProgress> UnlockProgress { get; }
        int Dependency { get; }
        
        // 업적 내용 출력 함수
        void PrintAchievementInfo();
    }

    // 해금 조건 타입 enum
    public enum UnlockConditionType
    {
        DefaultUnlock,          // 기본 제공
        CharacterWinCount,      // 캐릭터 승수 달성
        CharacterLevel,         // 캐릭터 레벨 달성
        SkillUsageCount,        // 스킬 사용 횟수
        RelicObtained,          // 유물 획득
        HealthBelowOne,         // 체력 1 이하 달성
        AttackWithBuff,         // 버프 상태에서 공격
        BuffDuration,           // 버프 지속시간
        KillWithBuff,           // 버프 상태에서 처치
        LightningBuffCount,     // 번개 버프 횟수
        BurnEffectCount,        // 화상 효과 횟수
        SlowEffectCount         // 둔화 효과 횟수
    }

    // 공통 데이터 구조
    [Serializable]
    public class UnlockCondition
    {
        public UnlockConditionType key;
        public int value;
    }

    [Serializable]
    public class UnlockProgress
    {
        public UnlockConditionType key;
        public int currentValue;
        public int maxValue;
    }

    // 캐릭터 데이터
    [Serializable]
    public class CharacterAchievement : IAchievementObject
    {
        public int characterId;
        public string characterName;
        public string characterDescription;
        public string unlockConditionDescription;
        public bool isUnlocked;
        public List<UnlockCondition> unlockConditions;
        public List<UnlockProgress> unlockProgress;

        // IAchievementObject 구현
        public int Id => characterId;
        public string Name => characterName;
        public string Description => characterDescription;
        public string UnlockConditionDescription => unlockConditionDescription;
        public bool IsUnlocked { get => isUnlocked; set => isUnlocked = value; }
        public List<UnlockCondition> UnlockConditions => unlockConditions;
        public List<UnlockProgress> UnlockProgress => unlockProgress;
        public int Dependency => -1; // 캐릭터는 의존성이 없음

        public void PrintAchievementInfo()
        {
            Debug.Log("=== 캐릭터 업적 정보 ===");
            Debug.Log($"ID: {characterId}");
            Debug.Log($"이름: {characterName}");
            Debug.Log($"설명: {characterDescription}");
            Debug.Log($"해금 조건 설명: {unlockConditionDescription}");
            Debug.Log($"해금 여부: {(isUnlocked ? "해금됨" : "미해금")}");
            Debug.Log($"의존성: 없음");
            
            Debug.Log("해금 조건:");
            for (int i = 0; i < unlockConditions.Count; i++)
            {
                Debug.Log($"  - {unlockConditions[i].key}: {unlockConditions[i].value}");
            }
            
            Debug.Log("진행도:");
            for (int i = 0; i < unlockProgress.Count; i++)
            {
                Debug.Log($"  - {unlockProgress[i].key}: {unlockProgress[i].currentValue}/{unlockProgress[i].maxValue}");
            }
            Debug.Log("========================");
        }
    }

    // 스킬 데이터
    [Serializable]
    public class SkillAchievement : IAchievementObject
    {
        public int skillId;
        public int dependency;
        public string skillName;
        public string skillIconAddressable;
        public string skillDescription;
        public List<SkillStat> skillStats;
        public string unlockConditionDescription;
        public bool isUnlocked;
        public List<UnlockCondition> unlockConditions;
        public List<UnlockProgress> unlockProgress;

        // IAchievementObject 구현
        public int Id => skillId;
        public string Name => skillName;
        public string Description => skillDescription;
        public string UnlockConditionDescription => unlockConditionDescription;
        public bool IsUnlocked { get => isUnlocked; set => isUnlocked = value; }
        public List<UnlockCondition> UnlockConditions => unlockConditions;
        public List<UnlockProgress> UnlockProgress => unlockProgress;
        public int Dependency => dependency;

        public void PrintAchievementInfo()
        {
            Debug.Log("=== 스킬 업적 정보 ===");
            Debug.Log($"ID: {skillId}");
            Debug.Log($"이름: {skillName}");
            Debug.Log($"설명: {skillDescription}");
            Debug.Log($"아이콘 Addressable: {skillIconAddressable}");
            Debug.Log($"해금 조건 설명: {unlockConditionDescription}");
            Debug.Log($"해금 여부: {(isUnlocked ? "해금됨" : "미해금")}");
            Debug.Log($"의존성: {(dependency == -1 ? "없음" : dependency.ToString())}");
            
            Debug.Log("스킬 스탯:");
            for (int i = 0; i < skillStats.Count; i++)
            {
                Debug.Log($"  - {skillStats[i].key}: {skillStats[i].value}");
            }
            
            Debug.Log("해금 조건:");
            for (int i = 0; i < unlockConditions.Count; i++)
            {
                Debug.Log($"  - {unlockConditions[i].key}: {unlockConditions[i].value}");
            }
            
            Debug.Log("진행도:");
            for (int i = 0; i < unlockProgress.Count; i++)
            {
                Debug.Log($"  - {unlockProgress[i].key}: {unlockProgress[i].currentValue}/{unlockProgress[i].maxValue}");
            }
            Debug.Log("======================");
        }
    }

    [Serializable]
    public class SkillStat
    {
        public string key;
        public string value;
    }

    // 유물 데이터
    [Serializable]
    public class RelicAchievement : IAchievementObject
    {
        public int relicId;
        public int dependency;
        public string relicName;
        public string relicIconAddressable;
        public string relicDescription;
        public List<RelicValue> relicValues;
        public string unlockConditionDescription;
        public bool isUnlocked;
        public List<UnlockCondition> unlockConditions;
        public List<UnlockProgress> unlockProgress;

        // IAchievementObject 구현
        public int Id => relicId;
        public string Name => relicName;
        public string Description => relicDescription;
        public string UnlockConditionDescription => unlockConditionDescription;
        public bool IsUnlocked { get => isUnlocked; set => isUnlocked = value; }
        public List<UnlockCondition> UnlockConditions => unlockConditions;
        public List<UnlockProgress> UnlockProgress => unlockProgress;
        public int Dependency => dependency;

        public void PrintAchievementInfo()
        {
            Debug.Log("=== 유물 업적 정보 ===");
            Debug.Log($"ID: {relicId}");
            Debug.Log($"이름: {relicName}");
            Debug.Log($"설명: {relicDescription}");
            Debug.Log($"아이콘 Addressable: {relicIconAddressable}");
            Debug.Log($"해금 조건 설명: {unlockConditionDescription}");
            Debug.Log($"해금 여부: {(isUnlocked ? "해금됨" : "미해금")}");
            Debug.Log($"의존성: {(dependency == -1 ? "없음" : dependency.ToString())}");
            
            Debug.Log("유물 값:");
            for (int i = 0; i < relicValues.Count; i++)
            {
                Debug.Log($"  - {relicValues[i].key}: {relicValues[i].value}");
            }
            
            Debug.Log("해금 조건:");
            for (int i = 0; i < unlockConditions.Count; i++)
            {
                Debug.Log($"  - {unlockConditions[i].key}: {unlockConditions[i].value}");
            }
            
            Debug.Log("진행도:");
            for (int i = 0; i < unlockProgress.Count; i++)
            {
                Debug.Log($"  - {unlockProgress[i].key}: {unlockProgress[i].currentValue}/{unlockProgress[i].maxValue}");
            }
            Debug.Log("======================");
        }
    }

    [Serializable]
    public class RelicValue
    {
        public string key;
        public string value;
    }

    // JSON 래퍼 클래스들
    [Serializable]
    public class CharacterAchievementData
    {
        public List<CharacterAchievement> characterAchievements;
    }

    [Serializable]
    public class SkillAchievementData
    {
        public List<SkillAchievement> skillAchievements;
    }

    [Serializable]
    public class RelicAchievementData
    {
        public List<RelicAchievement> relicAchievements;
    }

    // 업적 관리자
    public class JsonToAchivement
    {
        // 상수 정의
        private const string CHARACTER_JSON_NAME = "CharacterAchievementData.json";
        private const string SKILL_JSON_NAME = "SkillAchievementData.json";
        private const string RELIC_JSON_NAME = "RelicAchievementData.json";

        private const string RELIC_DATA_JSON_NAME = "RelicData.json";
        
        public const int CHARACTER_TYPE = 1;
        public const int SKILL_TYPE = 2;
        public const int RELIC_TYPE = 3;

        // 데이터 저장소
        private Dictionary<int, List<IAchievementObject>> achievementsByType;
        private Dictionary<int, IAchievementObject> allAchievementsById;

        // 싱글톤 패턴
        public static JsonToAchivement Instance { get; private set; }

        public JsonToAchivement()
        {
            Instance = this;
            Initialize();
        }

        private void Initialize()
        {
            achievementsByType = new Dictionary<int, List<IAchievementObject>>();
            allAchievementsById = new Dictionary<int, IAchievementObject>();

            LoadAllAchievements();
        }

        private void LoadAllAchievements()
        {
            string dataPath = Path.Combine(Application.dataPath, "Trieyes", "Data");

            // 캐릭터 데이터 로드
            LoadCharacterAchievements(dataPath);
            
            // 스킬 데이터 로드
            LoadSkillAchievements(dataPath);
            
            // 유물 데이터 로드
            LoadRelicAchievements(dataPath);
        }


        private void LoadCharacterAchievements(string dataPath)
        {
            string filePath = Path.Combine(dataPath, CHARACTER_JSON_NAME);
            
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                
                // JSON 설정: enum을 문자열로 변환
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
                };
                
                CharacterAchievementData data = JsonConvert.DeserializeObject<CharacterAchievementData>(jsonContent, settings);
                
                List<IAchievementObject> characterList = new List<IAchievementObject>();
                
                foreach (var character in data.characterAchievements)
                {
                    characterList.Add(character);
                    allAchievementsById[character.Id] = character;
                }
                
                achievementsByType[CHARACTER_TYPE] = characterList;
                Debug.Log($"캐릭터 업적 {characterList.Count}개 로드 완료");
            }
            else
            {
                Debug.LogError($"캐릭터 JSON 파일을 찾을 수 없습니다: {filePath}");
            }
        }

        private void LoadSkillAchievements(string dataPath)
        {
            string filePath = Path.Combine(dataPath, SKILL_JSON_NAME);
            
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                
                // JSON 설정: enum을 문자열로 변환
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
                };
                
                SkillAchievementData data = JsonConvert.DeserializeObject<SkillAchievementData>(jsonContent, settings);
                
                List<IAchievementObject> skillList = new List<IAchievementObject>();
                
                foreach (var skill in data.skillAchievements)
                {
                    skillList.Add(skill);
                    allAchievementsById[skill.Id] = skill;
                }
                
                achievementsByType[SKILL_TYPE] = skillList;
                Debug.Log($"스킬 업적 {skillList.Count}개 로드 완료");
            }
            else
            {
                Debug.LogError($"스킬 JSON 파일을 찾을 수 없습니다: {filePath}");
            }
        }

        private void LoadRelicAchievements(string dataPath)
        {
            string filePath = Path.Combine(dataPath, RELIC_JSON_NAME);
            
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                
                // JSON 설정: enum을 문자열로 변환
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
                };
                
                RelicAchievementData data = JsonConvert.DeserializeObject<RelicAchievementData>(jsonContent, settings);
                
                List<IAchievementObject> relicList = new List<IAchievementObject>();
                
                foreach (var relic in data.relicAchievements)
                {
                    relicList.Add(relic);
                    allAchievementsById[relic.Id] = relic;
                }
                
                achievementsByType[RELIC_TYPE] = relicList;
                Debug.Log($"유물 업적 {relicList.Count}개 로드 완료");
            }
            else
            {
                Debug.LogError($"유물 JSON 파일을 찾을 수 없습니다: {filePath}");
            }
        }

        // API 함수들

        /// <summary>
        /// 모든 타입의 업적을 반환합니다.
        /// </summary>
        /// <returns>타입별 업적 리스트</returns>
        public bool IsAchievementUnlocked(int id)
        {
            if (allAchievementsById.ContainsKey(id) && allAchievementsById[id].IsUnlocked)
            {
                return true;
            }
            return false;
        }

        public Dictionary<int, List<IAchievementObject>> GetAllAchievements()
        {
            return new Dictionary<int, List<IAchievementObject>>(achievementsByType);
        }

        /// <summary>
        /// 특정 타입의 업적 리스트를 반환합니다.
        /// </summary>
        /// <param name="type">업적 타입 (1: 캐릭터, 2: 스킬, 3: 유물)</param>
        /// <returns>해당 타입의 업적 리스트</returns>
        public List<IAchievementObject> GetAchievementsByType(int type)
        {
            if (achievementsByType.ContainsKey(type))
            {
                return new List<IAchievementObject>(achievementsByType[type]);
            }
            
            Debug.LogWarning($"타입 {type}에 해당하는 업적이 없습니다.");
            return new List<IAchievementObject>();
        }

        /// <summary>
        /// 해금된 모든 업적들을 반환합니다.
        /// </summary>
        /// <returns>타입별 해금된 업적 리스트</returns>
        public Dictionary<int, List<IAchievementObject>> GetUnlockedAchievements()
        {
            Dictionary<int, List<IAchievementObject>> unlockedAchievements = new Dictionary<int, List<IAchievementObject>>();
            
            foreach (var kvp in achievementsByType)
            {
                List<IAchievementObject> unlockedList = new List<IAchievementObject>();
                
                foreach (var achievement in kvp.Value)
                {
                    if (achievement.IsUnlocked)
                    {
                        unlockedList.Add(achievement);
                    }
                }
                
                unlockedAchievements[kvp.Key] = unlockedList;
            }
            
            return unlockedAchievements;
        }

        /// <summary>
        /// 특정 타입의 해금된 업적만 반환합니다.
        /// </summary>
        /// <param name="type">업적 타입</param>
        /// <returns>해당 타입의 해금된 업적 리스트</returns>
        public List<IAchievementObject> GetUnlockedAchievementsByType(int type)
        {
            List<IAchievementObject> unlockedList = new List<IAchievementObject>();
            
            if (achievementsByType.ContainsKey(type))
            {
                foreach (var achievement in achievementsByType[type])
                {
                    if (achievement.IsUnlocked)
                    {
                        unlockedList.Add(achievement);
                    }
                }
            }
            
            return unlockedList;
        }

        /// <summary>
        /// 특정 id에 종속받는 업적과 공통 타입의 해금된 업적들을 조회합니다.
        /// </summary>
        /// <param name="id">업적 ID</param>
        /// <returns>해당 ID의 업적 객체</returns>
        public List<IAchievementObject> GetUnlockedAchievementByIdByType(int type, int id)
        {
            List<IAchievementObject> unlockedList = new List<IAchievementObject>();
            foreach (var achievement in achievementsByType[type])
            {
                if ((achievement.Dependency == id || achievement.Dependency == -1) && achievement.IsUnlocked)
                {
                    unlockedList.Add(achievement);
                }
            }
            
            return unlockedList;
        }

        /// <summary>
        /// 모든 해금된 업적의 id 리스트를 가져옵니다.
        /// </summary>
        /// <returns>해금된 업적의 id 리스트</returns>
        public List<int> GetAllUnlockedAchievementIds()
        {
            List<int> unlockedIds = new List<int>();
            foreach (var kvp in allAchievementsById)
            {
                if (kvp.Value.IsUnlocked)
                {
                    unlockedIds.Add(kvp.Key);
                }
            }
            return unlockedIds;
        }

        /// <summary>
        /// 특정 타입의 해금된 업적의 id 리스트를 가져옵니다.
        /// </summary>
        /// <param name="type">업적 타입</param>
        /// <returns>해금된 업적의 id 리스트</returns>
        public List<int> GetUnlockedAchievementIdsByType(int type)
        {
            List<int> unlockedIds = new List<int>();
            foreach (var achievement in achievementsByType[type])
            {
                if (achievement.IsUnlocked)
                {
                    unlockedIds.Add(achievement.Id);
                }
            }
            return unlockedIds;
        }

        public void PrintAllAchievements()
        {
            foreach (var kvp in achievementsByType)
            {
                foreach (var achievement in kvp.Value)
                {
                    achievement.PrintAchievementInfo();
                }
            }
        }
    }
}
