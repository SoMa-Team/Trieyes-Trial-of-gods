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
            
            if (File.Exists(filePath) || true)
            {
                // TODO : 빌드용 하드코딩
                string jsonContent = "{\n  \"characterAchievements\": [\n    {\n      \"characterId\": 100001,\n      \"characterName\": \"용사\",\n      \"characterDescription\": \"고대의 전설에서 내려온 용사. 신들의 시험을 통과하기 위해 무수한 전투를 치러왔다. 그의 검은 악을 베어내고, 그의 방패는 동료들을 지킨다.\",\n      \"unlockConditionDescription\": \"게임 시작 시 기본 제공됩니다.\",\n      \"isUnlocked\": true,\n      \"unlockConditions\": [\n        {\n          \"key\": \"DefaultUnlock\",\n          \"value\": 0\n        }\n      ]\n    },\n    {\n      \"characterId\": 100002,\n      \"characterName\": \"마법사\",\n      \"characterDescription\": \"원소의 힘을 다루는 현명한 마법사. 불, 얼음, 번개의 힘을 자유자재로 조작하며, 전장에서 강력한 마법을 구사한다.\",\n      \"unlockConditionDescription\": \"{0}로 {1}승을 달성해야 합니다.\",\n      \"isUnlocked\": true,\n      \"unlockConditions\": [\n        {\n          \"key\": \"CharacterWinCount\",\n          \"value\": 10\n        }   \n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"CharacterWinCount\",\n          \"currentValue\": 10,\n          \"maxValue\": 10\n        }\n      ]\n    },\n    {\n      \"characterId\": 100003,\n      \"characterName\": \"궁수\",\n      \"characterDescription\": \"정확한 조준과 빠른 발사로 적을 제압하는 궁수. 원거리에서 안전하게 적을 처치하며, 특수한 화살로 다양한 효과를 발휘한다.\",\n      \"unlockConditionDescription\": \"{0}로 {1}승을 달성해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"CharacterWinCount\",\n          \"value\": 15\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"CharacterWinCount\",\n          \"currentValue\": 0,\n          \"maxValue\": 15\n        }\n      ]\n    },\n    {\n      \"characterId\": 100004,\n      \"characterName\": \"도적\",\n      \"characterDescription\": \"그림자 속에서 움직이는 신비로운 도적. 빠른 속도와 치명적인 공격으로 적을 기습하며, 독과 함정을 활용한 전술을 구사한다.\",\n      \"unlockConditionDescription\": \"{0}로 {1}승을 달성해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"CharacterWinCount\",\n          \"value\": 20\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"CharacterWinCount\",\n          \"currentValue\": 0,\n          \"maxValue\": 20\n        }\n      ]\n    },\n    {\n      \"characterId\": 100005,\n      \"characterName\": \"성기사\",\n      \"characterDescription\": \"신성한 힘으로 동료를 치유하고 보호하는 성기사. 강력한 방어력과 치유 능력으로 팀의 버팀목이 되며, 악을 정화하는 신성한 힘을 사용한다.\",\n      \"unlockConditionDescription\": \"{0}로 {1}승을 달성해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"CharacterWinCount\",\n          \"value\": 25\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"CharacterWinCount\",\n          \"currentValue\": 0,\n          \"maxValue\": 25\n        }\n      ]\n    }\n  ]\n}\n";
                
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
            
            if (File.Exists(filePath) || true)
            {
                // TODO : 빌드용 하드코딩
                string jsonContent = "{\n  \"skillAchievements\": [\n    {\n      \"skillId\": 1001,\n      \"dependency\": 100001,\n      \"skillName\": \"속성 부여\",\n      \"skillIconAddressable\": \"fire3\",\n      \"skillDescription\": \"무기에 {0} 속성을 부여하여 {1}초 동안 {2} 효과를 적용합니다.\",\n      \"unlockConditionDescription\": \"게임 시작 시 기본 제공됩니다.\",\n      \"isUnlocked\": true,\n      \"unlockConditions\": [\n        {\n          \"key\": \"DefaultUnlock\",\n          \"value\": 1\n        }\n      ]\n    },\n    {\n      \"skillId\": 1002,\n      \"dependency\": 100001,\n      \"skillName\": \"무기 속성 연계\",\n      \"skillIconAddressable\": \"fire3\",\n      \"skillDescription\": \"현재 검에 부여된 속성에 따라 다양한 효과가 발생합니다..\",\n      \"unlockConditionDescription\": \"{0} 유물을 획득해야 합니다.\",\n      \"isUnlocked\": true,\n      \"unlockConditions\": [\n        {\n          \"key\": \"RelicObtained\",\n          \"value\": 1\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"RelicObtained\",\n          \"currentValue\": 1,\n          \"maxValue\": 1\n        }\n      ]\n    },\n    {\n      \"skillId\": 1101,\n      \"dependency\": 100002,\n      \"skillName\": \"텔레포트\",\n      \"skillIconAddressable\": \"C002_S001\",\n      \"skillDescription\": \"0.5초 후 바라보는 방향으로 4거리 만큼 순간이동합니다.\",\n      \"unlockConditionDescription\": \"{0} 레벨 {1}을 달성해야 합니다.\",\n      \"isUnlocked\": true,\n      \"unlockConditions\": [\n        {\n          \"key\": \"CharacterLevel\",\n          \"value\": 5\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"CharacterLevel\",\n          \"currentValue\": 5,\n          \"maxValue\": 5\n        }\n      ]\n    },\n    {\n      \"skillId\": 1102,\n      \"dependency\": 100002,\n      \"skillName\": \"타이머 폭발\",\n      \"skillIconAddressable\": \"C002_S002\",\n      \"skillDescription\": \"1초 후 전방으로 공격력 [140]%의 마법 공격을 발사합니다. 맞은 대상이 죽지 않았다면 2초 후 사방 2거리에 [100]%의 광역 마법 공격이 발사됩니다.\",\n      \"unlockConditionDescription\": \"{0} 스킬을 {1}회 사용해야 합니다.\",\n      \"isUnlocked\": true,\n      \"unlockConditions\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"value\": 10\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"currentValue\": 10,\n          \"maxValue\": 10\n        }\n      ]\n    },\n    {\n      \"skillId\": 1103,\n      \"dependency\": 100002,\n      \"skillName\": \"공명 포탑\",\n      \"skillIconAddressable\": \"C002_S003\",\n      \"skillDescription\": \"현재 위치에 [10]초 동안 지속하는 공명 포탑을 생성합니다. 공명포탑은 어떠한 마법 공격이 닿았을 경우 전방 2 거리에 [100]% 광역 마법 공격을 발사합니다.\",\n      \"unlockConditionDescription\": \"{0} 스킬을 {1}회 사용해야 합니다.\",\n      \"isUnlocked\": true,\n      \"unlockConditions\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"value\": 15\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"currentValue\": 15,\n          \"maxValue\": 15\n        }\n      ]\n    }\n  ]\n}\n";
                
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
            // TODO : 빌드용 하드코딩
            string filePath = Path.Combine(dataPath, RELIC_JSON_NAME);
            
            if (File.Exists(filePath) || true)
            {
                string jsonContent = "{\n  \"relicAchievements\": [\n    {\n      \"relicId\": 2,\n      \"dependency\": 100001,\n      \"relicName\": \"용사의 가호\",\n      \"relicIconAddressable\": \"Relic_HeroBlessing\",\n      \"relicDescription\": \"원소부여 스킬이 천상 강림으로 바뀝니다. 천상 강림은 {0}초 동안 {1}% 공격력과 {2}% 방어력을 증가시킵니다.\",\n      \"unlockConditionDescription\": \"{0}로 {1}승을 달성해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"CharacterWinCount\",\n          \"value\": 5\n        }\n      ]\n    },\n    {\n      \"relicId\": 3,\n      \"dependency\": 100001,\n      \"relicName\": \"양손잡이\",\n      \"relicIconAddressable\": \"Relic_Ambidextrous\",\n      \"relicDescription\": \"원소부여 스킬의 쿨타임이 {0}초 감소합니다. 방패 밀쳐내기의 범위가 양방향으로 변합니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"쿨타임 감소\",\n          \"value\": \"2\"\n        }\n      ],\n      \"unlockConditionDescription\": \"{0} 스킬을 {1}회 사용해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"value\": 20\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"currentValue\": 0,\n          \"maxValue\": 20\n        }\n      ]\n    },\n    {\n      \"relicId\": 720003,\n      \"dependency\": 100001,\n      \"relicName\": \"죽음을 초월한 용사\",\n      \"relicIconAddressable\": \"Relic_TranscendedHero\",\n      \"relicDescription\": \"체력이 1 이하로 떨어지는 순간 {0}초 동안 무적 상태가 되고 체력을 {1}% 회복합니다. 이후 게임이 끝날 때까지 속성 부여 스킬의 쿨타임이 {2}초 감소합니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"무적 지속시간\",\n          \"value\": \"3\"\n        },\n        {\n          \"key\": \"체력 회복률\",\n          \"value\": \"20\"\n        },\n        {\n          \"key\": \"쿨타임 감소\",\n          \"value\": \"2\"\n        }\n      ],\n      \"unlockConditionDescription\": \"체력을 1 이하로 떨어뜨리기를 {0}회 해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"HealthBelowOne\",\n          \"value\": 10\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"HealthBelowOne\",\n          \"currentValue\": 0,\n          \"maxValue\": 10\n        }\n      ]\n    },\n    {\n      \"relicId\": 720004,\n      \"dependency\": 100001,\n      \"relicName\": \"RAC002\",\n      \"relicIconAddressable\": \"Relic_RAC002\",\n      \"relicDescription\": \"방패 밀쳐내기의 범위가 줄어들고 넉백이 사라집니다. 쿨타임이 {0}초로 감소합니다. 기본공격을 더 이상 사용할 수 없습니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"쿨타임\",\n          \"value\": \"3\"\n        }\n      ],\n      \"unlockConditionDescription\": \"{0} 스킬을 {1}회 사용해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"value\": 100\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"currentValue\": 0,\n          \"maxValue\": 100\n        }\n      ]\n    },\n    {\n      \"relicId\": 720005,\n      \"dependency\": 100001,\n      \"relicName\": \"RAC003\",\n      \"relicIconAddressable\": \"Relic_RAC003\",\n      \"relicDescription\": \"방패 돌진이 방어력에 비례하는 추가 피해를 입힙니다. 추가 피해는 방어력의 {0}%입니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"추가 피해 비율\",\n          \"value\": \"100\"\n        }\n      ],\n      \"unlockConditionDescription\": \"{0} 스킬을 {1}회 사용해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"value\": 75\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"currentValue\": 0,\n          \"maxValue\": 75\n        }\n      ]\n    },\n    {\n      \"relicId\": 720006,\n      \"dependency\": 100001,\n      \"relicName\": \"RAC004\",\n      \"relicIconAddressable\": \"Relic_RAC004\",\n      \"relicDescription\": \"전장의 포효가 사용자의 체력을 일부 회복시킵니다. 회복량은 최대 체력의 {0}%입니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"체력 회복률\",\n          \"value\": \"15\"\n        }\n      ],\n      \"unlockConditionDescription\": \"{0} 스킬을 {1}회 사용해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"value\": 50\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"currentValue\": 0,\n          \"maxValue\": 50\n        }\n      ]\n    },\n    {\n      \"relicId\": 720007,\n      \"dependency\": 100001,\n      \"relicName\": \"RAC005\",\n      \"relicIconAddressable\": \"Relic_RAC005\",\n      \"relicDescription\": \"속성 부여가 무작위 속성을 부여하는 대신 천상 속성을 부여합니다. 천상 속성은 {0}초 동안 지속됩니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"천상 속성 지속시간\",\n          \"value\": \"10\"\n        }\n      ],\n      \"unlockConditionDescription\": \"{0} 스킬을 {1}회 사용해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"value\": 200\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"currentValue\": 0,\n          \"maxValue\": 200\n        }\n      ]\n    },\n    {\n      \"relicId\": 720008,\n      \"dependency\": 100001,\n      \"relicName\": \"RAC006\",\n      \"relicIconAddressable\": \"Relic_RAC006\",\n      \"relicDescription\": \"속성이 부여되면 공격할 때마다 전방으로 해당 속성의 검기를 발사합니다. 검기는 {0}의 피해를 입힙니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"검기 피해량\",\n          \"value\": \"공격력의 60%\"\n        }\n      ],\n      \"unlockConditionDescription\": \"속성 부여 상태에서 공격을 {0}회 해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"AttackWithBuff\",\n          \"value\": 500\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"AttackWithBuff\",\n          \"currentValue\": 0,\n          \"maxValue\": 500\n        }\n      ]\n    },\n    {\n      \"relicId\": 720009,\n      \"dependency\": 100001,\n      \"relicName\": \"RAC007\",\n      \"relicIconAddressable\": \"Relic_RAC007\",\n      \"relicDescription\": \"속성이 부여되는 동안 함께 전투를 도와주는 해당 속성의 정령이 소환됩니다. 정령은 {0}초마다 공격합니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"정령 공격 간격\",\n          \"value\": \"2\"\n        }\n      ],\n      \"unlockConditionDescription\": \"속성 부여 상태를 {0}초 유지해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"BuffDuration\",\n          \"value\": 1000\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"BuffDuration\",\n          \"currentValue\": 0,\n          \"maxValue\": 1000\n        }\n      ]\n    },\n    {\n      \"relicId\": 720010,\n      \"dependency\": 100001,\n      \"relicName\": \"RAC008\",\n      \"relicIconAddressable\": \"Relic_RAC008\",\n      \"relicDescription\": \"속성 부여 상태에서 적을 죽일 때마다 지속시간이 {0}초 증가합니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"지속시간 증가\",\n          \"value\": \"0.1\"\n        }\n      ],\n      \"unlockConditionDescription\": \"속성 부여 상태에서 적을 {0}회 처치해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"KillWithBuff\",\n          \"value\": 100\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"KillWithBuff\",\n          \"currentValue\": 0,\n          \"maxValue\": 100\n        }\n      ]\n    },\n    {\n      \"relicId\": 720011,\n      \"dependency\": 100001,\n      \"relicName\": \"RAC009\",\n      \"relicIconAddressable\": \"Relic_RAC009\",\n      \"relicDescription\": \"속성 부여 스킬을 사용할 때마다 자신의 주위를 공전하는 별을 생성합니다. 별에 닿을 때마다 적은 {0}의 피해를 받습니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"별 피해량\",\n          \"value\": \"공격력의 40%\"\n        }\n      ],\n      \"unlockConditionDescription\": \"{0} 스킬을 {1}회 사용해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"value\": 300\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"SkillUsageCount\",\n          \"currentValue\": 0,\n          \"maxValue\": 300\n        }\n      ]\n    },\n    {\n      \"relicId\": 720012,\n      \"dependency\": 100001,\n      \"relicName\": \"RAC010\",\n      \"relicIconAddressable\": \"Relic_RAC010\",\n      \"relicDescription\": \"속성 부여가 번개 속성으로 고정됩니다. 번개 속성이 부여됐을 때 공격속도가 {0}% 증가합니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"공격속도 증가\",\n          \"value\": \"100\"\n        }\n      ],\n      \"unlockConditionDescription\": \"번개 속성 부여를 {0}회 해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"LightningBuffCount\",\n          \"value\": 150\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"LightningBuffCount\",\n          \"currentValue\": 0,\n          \"maxValue\": 150\n        }\n      ]\n    },\n    {\n      \"relicId\": 720013,\n      \"dependency\": 100001,\n      \"relicName\": \"RAC011\",\n      \"relicIconAddressable\": \"Relic_RAC011\",\n      \"relicDescription\": \"속성 부여가 불 속성으로 고정됩니다. 화상을 입은 적이 다시 화상을 입는 경우 남은 화상 피해량의 {0}%를 즉시 입으며 화상의 지속시간이 초기화됩니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"즉시 피해 비율\",\n          \"value\": \"20\"\n        }\n      ],\n      \"unlockConditionDescription\": \"화상 효과를 {0}회 적용해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"BurnEffectCount\",\n          \"value\": 200\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"BurnEffectCount\",\n          \"currentValue\": 0,\n          \"maxValue\": 200\n        }\n      ]\n    },\n    {\n      \"relicId\": 720014,\n      \"dependency\": 100001,\n      \"relicName\": \"RAC012\",\n      \"relicIconAddressable\": \"Relic_RAC012\",\n      \"relicDescription\": \"속성부여가 얼음 속성으로 고정됩니다. 둔화가 걸린 적이 다시 둔화에 걸리는 경우 해당 적의 방어력이 {0}% 감소합니다.\",\n      \"relicValues\": [\n        {\n          \"key\": \"방어력 감소율\",\n          \"value\": \"25\"\n        }\n      ],\n      \"unlockConditionDescription\": \"둔화 효과를 {0}회 적용해야 합니다.\",\n      \"isUnlocked\": false,\n      \"unlockConditions\": [\n        {\n          \"key\": \"SlowEffectCount\",\n          \"value\": 180\n        }\n      ],\n      \"unlockProgress\": [\n        {\n          \"key\": \"SlowEffectCount\",\n          \"currentValue\": 0,\n          \"maxValue\": 180\n        }\n      ]\n    }\n  ]\n}\n";
                
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
