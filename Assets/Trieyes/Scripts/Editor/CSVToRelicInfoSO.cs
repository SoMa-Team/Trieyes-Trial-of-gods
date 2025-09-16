#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using RelicSystem; // RelicDataSO 네임스페이스
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class CSVToRelicInfoSOImporter
{
    [MenuItem("Tools/CSVToRelicInfoSO")]
    public static void ImportFromCSV()
    {
        // 1. CSV 파일 경로
        string csvPath = "Assets/Trieyes/Data/RelicInfoData/RelicInfoData.csv";
        // 2. SO 저장 폴더
        string soOutputPath = "Assets/Trieyes/ScriptableObjects/RelicInfos";

        // 3. CSV 읽기
        var lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("CSV 파일이 비어있거나 헤더만 있습니다.");
            return;
        }

        var headers = lines[0].Split(',');
        int idx_id = Array.IndexOf(headers, "id");
        int idx_name = Array.IndexOf(headers, "name");
        int idx_icon = Array.IndexOf(headers, "icon");
        int idx_description = Array.IndexOf(headers, "description");
        int idx_attackComponentIDs = Array.IndexOf(headers, "attackComponentIDs");
        int idx_filterAttackIDs = Array.IndexOf(headers, "filterAttackIDs");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))continue;
            var values = Utils.CsvUtils.SplitCsvLine(lines[i]).ToArray();

            if (values.Length < headers.Length) continue;

            // 경로/파일명
            string soName = values[idx_name].Trim();
            string assetPath = Path.Combine(soOutputPath, $"{soName}.asset");

            // 기존 SO 불러오기 or 새로 생성
            RelicDataSO relic =
                AssetDatabase.LoadAssetAtPath<RelicDataSO>(assetPath.Replace(Application.dataPath, "Assets"));
            bool isNew = false;
            if (relic == null)
            {
                relic = ScriptableObject.CreateInstance<RelicDataSO>();
                isNew = true;
            }

            // id
            if (int.TryParse(values[idx_id], out int relicId))
                relic.id = relicId;
            else
                Debug.LogWarning($"{soName} id 파싱 실패: {values[idx_id]}");

            // name
            relic.name = values[idx_name];

            // icon
            Addressables.LoadAssetAsync<Sprite>($"Assets/Trieyes/Addressable/Icons/Relics/{values[idx_icon]}")
                .Completed += handle =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    relic.icon = null;
                relic.icon = handle.Result;
            };

            // description
            relic.description = values[idx_description];

            // attackComponentIDs : "1|2|3" → List<int>
            relic.attackComponentIDs = ParseIntListOrNull(values[idx_attackComponentIDs]);

            // filterAttackIDs : "1|2|3" or 빈 값(null) → List<int>? (null if no filter)
            relic.filterAttackIDs = string.IsNullOrWhiteSpace(values[idx_filterAttackIDs])
                ? null
                : ParseIntListOrNull(values[idx_filterAttackIDs]);

            // 저장
            if (isNew)
                AssetDatabase.CreateAsset(relic, assetPath.Replace(Application.dataPath, "Assets"));
            else
                EditorUtility.SetDirty(relic);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("RelicDataSO 생성/덮어쓰기 완료!");
    }

    // "1|2|3" → List<int>, 빈 값이면 null
    private static List<int> ParseIntListOrNull(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return value.Split('|').Select(s =>
        {
            if (int.TryParse(s, out var v)) return v;
            Debug.LogWarning($"정수 변환 실패: {s}");
            return -1;
        }).ToList();
    }
}
#endif