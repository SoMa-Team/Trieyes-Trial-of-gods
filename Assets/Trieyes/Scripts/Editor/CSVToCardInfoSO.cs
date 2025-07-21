#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CardSystem;
using Utils;
using TMPro;
using System;

public static class CSVToCardInfoSOImporter
{
    private const string TMP_FONT_ASSET_PATH = "Assets/Trieyes/Font/SDF/DNFForgedBlade-Bold SDF.asset";
    [MenuItem("Tools/CSVToCardInfoSO")]
    public static void ImportFromCSV()
    {
        // 1. CSV 파일 선택
        string csvPath = "Assets/Trieyes/Data/CardInfoData/CardInfoData.csv";

        // 2. SO 저장 폴더 선택
        string soOutputPath = "Assets/Trieyes/ScriptableObjects/CardInfos";

        // 3. CSV 읽기
        var lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("CSV 파일이 비어있거나 헤더만 있습니다.");
            return;
        }

        var headers = lines[0].Split(',');
        int idx_Id = System.Array.IndexOf(headers, "Id");
        int idx_soName = System.Array.IndexOf(headers, "soName");
        int idx_cardName = System.Array.IndexOf(headers, "cardName");
        int idx_rarity = System.Array.IndexOf(headers, "rarity");
        int idx_properties = System.Array.IndexOf(headers, "properties");
        int idx_illustration = System.Array.IndexOf(headers, "illustration");
        int idx_cardDescription = System.Array.IndexOf(headers, "cardDescription");
        int idx_eventTypes = System.Array.IndexOf(headers, "eventTypes");
        int idx_baseParams = System.Array.IndexOf(headers, "baseParams");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var values = CsvUtils.SplitCsvLine(lines[i]).ToArray();

            if (values.Length < headers.Length) continue;
            int Id = int.Parse(values[idx_Id]);
            string cardName = values[idx_cardName];

            // SO 파일 경로 생성
            string soName = values[idx_soName].Trim();
            string assetPath = Path.Combine(soOutputPath, $"{soName}.asset");

            // 기존 SO가 있으면 불러오고, 없으면 새로 생성
            CardInfo card = AssetDatabase.LoadAssetAtPath<CardInfo>(assetPath.Replace(Application.dataPath, "Assets"));
            bool isNew = false;
            if (card == null)
            {
                card = ScriptableObject.CreateInstance<CardInfo>();
                isNew = true;
            }
            
            card.Id = Id;
            card.cardName = cardName;

            // rarity
            if (System.Enum.TryParse(typeof(Rarity), values[idx_rarity], out object rarityValue))
                card.rarity = (Rarity)rarityValue;

            // properties
            card.properties = values[idx_properties]
                .Split('|')
                .Select(s => (Property)System.Enum.Parse(typeof(Property), s))
                .ToArray();

            // illustration
            card.illustration = Resources.Load<Sprite>($"CardIllustration/{values[idx_illustration]}");
            if (card.illustration == null)
                Debug.LogWarning($"{cardName}: CardIllustration/{values[idx_illustration]} Sprite를 Resources에서 못 찾음!");

            card.cardDescription = values[idx_cardDescription];
            Debug.Log(card.cardDescription);

            // eventTypes
            card.eventTypes = values[idx_eventTypes]
                .Split('|')
                .Select(s => (Utils.EventType)System.Enum.Parse(typeof(Utils.EventType), s))
                .ToList();
            
            if (idx_baseParams >= 0 && !string.IsNullOrWhiteSpace(values[idx_baseParams]))
                card.baseParams = values[idx_baseParams].Split('|').Select(x => x.Trim()).ToList();
            else
                card.baseParams = new List<string>();
            
            card.paramWordRanges = ParseParamWordRangesWithTMP(card.cardDescription, card.baseParams);

            if (isNew)
                AssetDatabase.CreateAsset(card, assetPath.Replace(Application.dataPath, "Assets"));
            else
                EditorUtility.SetDirty(card);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CardInfoSO 생성/덮어쓰기 완료!");
    }
    
    private static List<ParamWordRange> ParseParamWordRangesWithTMP(string description, List<string> baseParams)
    {
        // --- 에셋에서 TMP 폰트 로드 ---
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TMP_FONT_ASSET_PATH);
        if (fontAsset == null)
        {
            Debug.LogError($"TMP 폰트 에셋을 찾을 수 없습니다: {TMP_FONT_ASSET_PATH}");
            return new List<ParamWordRange>();
        }

        // --- 임시 Canvas, TMP 객체 생성 ---
        var canvasGo = new GameObject("TMP_Parser_Canvas", typeof(Canvas));
        canvasGo.hideFlags = HideFlags.HideAndDontSave;
        var go = new GameObject("TMPDescParser");
        go.transform.SetParent(canvasGo.transform);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = fontAsset;
        tmp.text = description;
        tmp.ForceMeshUpdate();

        Debug.Log($"[TMP] text: {tmp.text} / wordCount: {tmp.textInfo.wordCount}");

        // Step 1. 파라미터 등장 순서대로 (ex. {0}, {1}, ...)
        var paramMatches = new List<(int paramIdx, int origWordIdx)>();
        for (int i = 0; i < tmp.textInfo.wordCount; i++)
        {
            string word = tmp.textInfo.wordInfo[i].GetWord();
            Debug.Log($"[TMP] word: {word}");
            var match = System.Text.RegularExpressions.Regex.Match(word, @"^\d+$");
            if (match.Success)
            {
                int paramIdx = int.Parse(match.Groups[0].Value); // Groups[0], 그룹 캡처 없음
                paramMatches.Add((paramIdx, i));
            }
        }

        foreach (var paramIdx in paramMatches)
        {
            Debug.Log($"[TMP] paramIdx: {paramIdx}");
        }

        // Step 2. 치환 단어 수 계산 (baseParams: "이동 속도|10" 등)
        var paramWordCounts = new List<int>();
        foreach (var (paramIdx, _) in paramMatches)
        {
            // baseParams의 paramIdx 번째 값을 단어(공백 기준)로 나눔
            if (baseParams != null && paramIdx < baseParams.Count)
            {
                string val = baseParams[paramIdx];
                Debug.Log($"[TMP] paramIdx: {paramIdx}, value: {val}");
                try
                {
                    int count = string.IsNullOrWhiteSpace(val) ? 1 : val.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                    Debug.Log($"[TMP] count: {count}");
                    paramWordCounts.Add(count);
                    Debug.Log($"Try 진입");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[TMP] paramIdx: {paramIdx} 예외 발생: {ex}");
                }
            }
            else
            {
                Debug.LogError($"[TMP] 파싱에서 문제가 발생했습니다.");
            }
        }

        foreach (var paramWordCount in paramWordCounts)
        {
            Debug.Log($"[TMP] paramWordCount: {paramWordCount}");
        }

        // Step 3. 실제 인덱스 범위 계산
        var ranges = new List<ParamWordRange>();
        int offset = 0;
        for (int i = 0; i < paramMatches.Count; i++)
        {
            int origIdx = paramMatches[i].origWordIdx;
            int count = paramWordCounts[i];
            int start = origIdx + offset;
            int end = start + count - 1;
            Debug.Log($"[TMP] start: {start}, end: {end}");
            ranges.Add(new ParamWordRange { start = start, end = end });
            offset += count - 1; // 현재 파라미터가 1보다 크면 뒤 인덱스가 밀림
        }

        GameObject.DestroyImmediate(canvasGo);
        return ranges;
    }
}
#endif
