using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Stats; // StatType, StatValuePair가 포함된 네임스페이스

public class CSVToStatPresetSO : EditorWindow
{
    [MenuItem("Tools/CSV to StatPreset")]
    static void ConvertCSVToStatPresets()
    {
        // 1. 읽어올 CSV 파일 경로를 직접 지정합니다.
        // 이 경로와 파일명을 실제 파일에 맞게 수정해야 합니다.
        string filePath = "Assets/Trieyes/Data/StatPresetsData/StatPresets.csv";

        // 지정된 경로에 파일이 있는지 확인
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[StatImporter] CSV 파일을 찾을 수 없습니다. 경로를 확인하세요: {filePath}");
            return;
        }

        // 2. 파일의 모든 라인 읽기
        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length == 0)
        {
            Debug.LogWarning($"[StatImporter] CSV 파일이 비어있습니다: {filePath}");
            return;
        }

        // 캐릭터별 StatInfo를 저장할 Dictionary
        Dictionary<string, List<StatValuePair>> charStatDict = new Dictionary<string, List<StatValuePair>>();
        
        // 3. 각 라인을 순회하며 데이터 파싱
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] cells = line.Split(',');
            if (cells.Length < 2) continue;

            string charName = cells[0].Trim();

            if (!charStatDict.ContainsKey(charName))
            {
                charStatDict[charName] = new List<StatValuePair>();
            }

            // 4. 스탯 데이터 처리 (두 번째 컬럼부터)
            for (int j = 1; j < cells.Length; j++)
            {
                if (int.TryParse(cells[j], out int value))
                {
                    StatType type = (StatType)(j - 1);
                    
                    if (System.Enum.IsDefined(typeof(StatType), type))
                    {
                        charStatDict[charName].Add(new StatValuePair
                        {
                            type = type,
                            value = value
                        });
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid StatType index {j - 1} at column {j} for '{charName}'.");
                    }
                }
            }
        }

        // 5. ScriptableObject 생성
        string outDir = "Assets/Trieyes/ScriptableObjects/StatsPresets/";
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }

        foreach (var pair in charStatDict)
        {
            StatPresetSO asset = ScriptableObject.CreateInstance<StatPresetSO>();
            asset.characterName = pair.Key;
            asset.stats = pair.Value;

            string assetPath = $"{outDir}{pair.Key}StatPreset.asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[StatImporter] CSV import complete! Created {charStatDict.Count} presets from {Path.GetFileName(filePath)}.");
    }
}