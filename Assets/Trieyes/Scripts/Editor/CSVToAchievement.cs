#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using GamePlayer;
using System;

public static class CSVToAchievementImporter
{
    [MenuItem("Tools/CSVToAchievement")]
    public static void ImportFromCSV()
    {
        // 1. CSV 파일 경로
        string csvPath = "Assets/Trieyes/Data/AchievementData/AchievementData.csv";
        
        // 2. SO 저장 경로
        string soOutputPath = "Assets/Trieyes/ScriptableObjects/Achievements";
        string assetPath = Path.Combine(soOutputPath, "Hero_AchievementDatabase.asset");

        // CSV 파일 존재 확인
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: {csvPath}");
            return;
        }

        // 3. CSV 읽기
        var lines = File.ReadAllLines(csvPath);
        Debug.Log($"CSV 파일 읽기 완료: {lines.Length}줄");
        
        if (lines.Length < 2)
        {
            Debug.LogError("CSV 파일이 비어있거나 헤더만 있습니다.");
            return;
        }

        var headers = lines[0].Split(',');
        Debug.Log($"헤더: {string.Join(", ", headers)}");
        
        int idx_achievementType = Array.IndexOf(headers, "achievementType");
        int idx_achievementID = Array.IndexOf(headers, "achievementID");
        int idx_achievementName = Array.IndexOf(headers, "achievementName");
        int idx_achievementDescription = Array.IndexOf(headers, "achievementDescription");
        int idx_achievementProgressCurrent = Array.IndexOf(headers, "achievementProgressCurrent");
        int idx_achievementProgressMax = Array.IndexOf(headers, "achievementProgressMax");
        int idx_unlockElementID = Array.IndexOf(headers, "unlockElementID");
        int idx_addressableKey = Array.IndexOf(headers, "AddressableKey");
        
        // 폴더가 없으면 생성
        if (!Directory.Exists(soOutputPath))
        {
            Directory.CreateDirectory(soOutputPath);
            Debug.Log($"폴더 생성: {soOutputPath}");
        }

        // 기존 SO 불러오기 or 새로 생성
        AchievementDatabaseSO database = AssetDatabase.LoadAssetAtPath<AchievementDatabaseSO>(assetPath.Replace(Application.dataPath, "Assets"));
        bool isNew = false;
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<AchievementDatabaseSO>();
            isNew = true;
            Debug.Log("새로운 AchievementDatabaseSO 생성");
        }
        else
        {
            Debug.Log("기존 AchievementDatabaseSO 로드");
        }

        // 리스트 초기화
        database.achievementDataList = new List<SerializableAchievementData>();
        database.unlockedAchievementIds = new List<int>();

        int processedCount = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) 
            {
                Debug.Log($"빈 줄 건너뛰기: {i}번째 줄");
                continue;
            }
            
            var values = lines[i].Split(',');
            Debug.Log($"줄 {i} 파싱: {lines[i]}");

            if (values.Length < headers.Length) 
            {
                Debug.LogWarning($"줄 {i}: 값 개수가 부족합니다. 예상: {headers.Length}, 실제: {values.Length}");
                continue;
            }

            AchievementData achievement = new AchievementData();

            // achievementType
            if (int.TryParse(values[idx_achievementType], out int typeValue))
                achievement.achievementType = (AchievementType)typeValue;
            else
                Debug.LogWarning($"줄 {i} achievementType 파싱 실패: {values[idx_achievementType]}");

            // achievementID
            if (int.TryParse(values[idx_achievementID], out int id))
                achievement.achievementID = id;
            else
                Debug.LogWarning($"줄 {i} achievementID 파싱 실패: {values[idx_achievementID]}");

            // achievementName
            achievement.achievementName = values[idx_achievementName];

            // achievementDescription
            achievement.achievementDescription = values[idx_achievementDescription];

            // achievementProgressCurrent
            if (int.TryParse(values[idx_achievementProgressCurrent], out int current))
                achievement.achievementProgressCurrent = current;
            else
                Debug.LogWarning($"줄 {i} achievementProgressCurrent 파싱 실패: {values[idx_achievementProgressCurrent]}");

            // achievementProgressMax
            if (int.TryParse(values[idx_achievementProgressMax], out int max))
                achievement.achievementProgressMax = max;
            else
                Debug.LogWarning($"줄 {i} achievementProgressMax 파싱 실패: {values[idx_achievementProgressMax]}");

            // unlockElementID
            if (int.TryParse(values[idx_unlockElementID], out int unlockId))
                achievement.unlockElementID = unlockId;
            else
                Debug.LogWarning($"줄 {i} unlockElementID 파싱 실패: {values[idx_unlockElementID]}");

            // addressableKey
            if (idx_addressableKey >= 0 && idx_addressableKey < values.Length)
            {
                achievement.AddressableKey = values[idx_addressableKey];
            }
            else
            {
                achievement.AddressableKey = "";
            }

            // 리스트에 추가
            database.achievementDataList.Add(new SerializableAchievementData
            {
                id = achievement.achievementID,
                data = achievement
            });
            
            // 해금된 업적이면 해금 리스트에 추가
            if (achievement.IsUnlocked)
            {
                database.unlockedAchievementIds.Add(achievement.achievementID);
            }
            
            processedCount++;
        }

        Debug.Log($"총 {processedCount}개의 업적 데이터 처리 완료");

        // 저장
        if (isNew)
        {
            AssetDatabase.CreateAsset(database, assetPath.Replace(Application.dataPath, "Assets"));
            Debug.Log($"새 SO 파일 생성: {assetPath}");
        }
        else
        {
            EditorUtility.SetDirty(database);
            Debug.Log("기존 SO 파일 업데이트");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"AchievementDatabaseSO 생성/덮어쓰기 완료! 총 {database.achievementDataList.Count}개의 업적 데이터가 로드되었습니다.");
    }
}
#endif