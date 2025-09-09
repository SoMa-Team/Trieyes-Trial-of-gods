#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using NodeStage; // StageInfoSO, StageType

public static class CSVToStageInfoSOImporter
{
    private const string CSV_PATH          = "Assets/Trieyes/Data/StageInfoData/StageInfoData.csv";
    private const string SO_OUTPUT_FOLDER  = "Assets/Trieyes/ScriptableObjects/StageInfos";
    private const string ICON_RES_PREFIX   = "StageIcons/";

    [MenuItem("Tools/CSVToStageInfoSO")]
    public static void ImportFromCSV()
    {
        if (!File.Exists(CSV_PATH))
        {
            Debug.LogError($"[StageImporter] CSV 파일을 찾을 수 없습니다: {CSV_PATH}");
            return;
        }

        EnsureFolder(SO_OUTPUT_FOLDER);

        var lines = File.ReadAllLines(CSV_PATH)
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .ToList();
        if (lines.Count < 2)
        {
            Debug.LogError("[StageImporter] 파일이 비어있거나 헤더만 있습니다.");
            return;
        }

        // ---- 헤더 검증: 정확히 SOName,type,name,Icon ----
        var header = SplitCSV(lines[0]);
        if (header.Count != 4 ||
            header[0].Trim() != "SOName" ||
            header[1].Trim() != "type"   ||
            header[2].Trim() != "name"   ||
            header[3].Trim() != "Icon")
        {
            Debug.LogError("[StageImporter] 헤더는 정확히 \"SOName,type,name,Icon\" 이어야 합니다.");
            return;
        }

        int created = 0, updated = 0;
        for (int i = 1; i < lines.Count; i++)
        {
            var cols = SplitCSV(lines[i]);
            if (cols.Count != 4)
            {
                Debug.LogError($"[StageImporter] (line {i+1}) 열 개수는 4개여야 합니다. 중단합니다.");
                return;
            }

            string soName   = cols[0].Trim();
            string typeStr  = cols[1].Trim();
            string dispName = cols[2].Trim(); // SO 내부의 string name 필드에 그대로 설정
            string iconKey  = cols[3].Trim();

            if (string.IsNullOrEmpty(soName) || string.IsNullOrEmpty(typeStr) ||
                string.IsNullOrEmpty(dispName) || string.IsNullOrEmpty(iconKey))
            {
                Debug.LogError($"[StageImporter] (line {i+1}) 빈 값이 존재합니다. 중단합니다.");
                return;
            }

            if (!Enum.TryParse(typeStr, true, out StageType stageType))
            {
                Debug.LogError($"[StageImporter] (line {i+1}) StageType 파싱 실패: '{typeStr}'. 중단합니다.");
                return;
            }

            // 아이콘 로드 (Sheet:Sprite 형식 또는 단일 키)
            var iconSprite = LoadIcon(iconKey);
            if (iconSprite == null)
            {
                Debug.LogError($"[StageImporter] (line {i+1}) 아이콘을 찾을 수 없습니다: Resources/{ICON_RES_PREFIX}{iconKey}. 중단합니다.");
                return;
            }

            // 에셋 경로
            string fileName  = SanitizeFileName(soName) + ".asset";
            string assetPath = $"{SO_OUTPUT_FOLDER}/{fileName}";

            // SO 로드/생성
            var so = AssetDatabase.LoadAssetAtPath<StageInfoSO>(assetPath);
            bool isNew = false;
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<StageInfoSO>();
                AssetDatabase.CreateAsset(so, assetPath);
                isNew = true;
            }

            // ---- SO 필드 주입: 네가 정의한 필드에 '정확히' 대입 ----
            so.type = stageType;
            so.name = dispName;                    // StageInfoSO의 string name 필드
            ((UnityEngine.Object)so).name = soName; // 에셋 표시 이름(Inspector 상단명)
            so.icon = iconSprite;

            EditorUtility.SetDirty(so);
            if (isNew) created++; else updated++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[StageImporter] 완료: 생성 {created}, 갱신 {updated}");
    }

    // -------- Icon 로드: "Sheet:Sprite" 또는 단일 키 --------
    private static Sprite LoadIcon(string key)
    {
        int colon = key.IndexOf(':');
        if (colon >= 0)
        {
            string sheet = key.Substring(0, colon).Trim();
            string spriteName = key.Substring(colon + 1).Trim();
            var sprites = Resources.LoadAll<Sprite>(ICON_RES_PREFIX + sheet);
            return Array.Find(sprites, s => s != null && s.name == spriteName);
        }
        return Resources.Load<Sprite>(ICON_RES_PREFIX + key.Trim());
    }

    // -------- CSV 파서 (콤마/따옴표 이스케이프) --------
    private static List<string> SplitCSV(string line)
    {
        var res = new List<string>();
        if (string.IsNullOrEmpty(line)) return res;

        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"') { sb.Append('\"'); i++; }
                else inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                res.Add(sb.ToString());
                sb.Clear();
            }
            else sb.Append(c);
        }
        res.Add(sb.ToString());
        return res;
    }

    // -------- 유틸 --------
    private static void EnsureFolder(string folder)
    {
        string path = folder.Replace("\\", "/");
        if (!path.StartsWith("Assets"))
            throw new Exception($"폴더는 'Assets' 아래여야 합니다: {path}");

        var parts = path.Split('/');
        string cur = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            string next = parts[i];
            if (string.IsNullOrEmpty(next)) continue;
            string combined = $"{cur}/{next}";
            if (!AssetDatabase.IsValidFolder(combined))
                AssetDatabase.CreateFolder(cur, next);
            cur = combined;
        }
    }

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c.ToString(), "_");
        return name.Trim();
    }
}
#endif
