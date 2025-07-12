using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Stats;

public class CSVToStatPresetSO : EditorWindow
{
    [MenuItem("Tools/CSVToStatPreset")]
    static void ImportMultipleCSVs()
    {
        // 다중 파일 선택
        string folderPath = "Assets/Trieyes/Data/StatPresetsData";
        if (string.IsNullOrEmpty(folderPath)) return;

        string[] csvPaths = Directory.GetFiles(folderPath, "*.csv");
        if (csvPaths.Length == 0)
        {
            Debug.LogWarning("선택한 폴더에 csv 파일이 없습니다.");
            return;
        }

        // 캐릭터별 통합 StatInfo 저장소
        Dictionary<string, List<StatValuePair>> charStatDict = new();

        foreach (string csvPath in csvPaths)
        {
            string[] lines = File.ReadAllLines(csvPath);
            if (lines.Length < 2) continue;

            string[] headers = lines[0].Split(',');

            for (int i = 1; i < lines.Length; i++)
            {
                string[] cells = lines[i].Split(',');
                if (cells.Length == 0 || string.IsNullOrWhiteSpace(cells[0])) continue;

                string charName = cells[0].Trim();

                if (!charStatDict.ContainsKey(charName))
                    charStatDict[charName] = new List<StatValuePair>();

                // 1번째 컬럼(이름) 제외
                for (int j = 1; j < cells.Length; j++)
                {
                    if (int.TryParse(cells[j], out int value) && value != 0)
                    {
                        string statName = headers[j].Trim();
                        if (System.Enum.TryParse<StatType>(statName, out var type))
                        {
                            charStatDict[charName].Add(new StatValuePair
                            {
                                type = type,
                                value = value
                            });
                        }
                        else
                        {
                            Debug.LogWarning($"{statName} : StatType 파싱 실패 ({csvPath})");
                        }
                    }
                }
            }
        }

        // ScriptableObject 생성
        string outDir = "Assets/Trieyes/ScriptableObjects/StatsPresets/";
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

        foreach (var pair in charStatDict)
        {
            var asset = ScriptableObject.CreateInstance<StatPresetSO>();
            asset.characterName = pair.Key;
            asset.stats = pair.Value;

            string assetPath = $"{outDir}{pair.Key}StatPreset.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[StatImporter] 모든 CSV 임포트/통합 완료! ({charStatDict.Count}명)");
    }
}