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
using System.Text.RegularExpressions;

public static class CSVToCardInfoSOImporter
{
    private static readonly Regex ParamTokenRegex = new Regex(@"^\d+$");
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

            card.cardDescription = values[idx_cardDescription].Replace("\\n", "\n");
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
        var paramRanges = new List<ParamWordRange>();

        for (int i = 0; i < tmp.textInfo.wordCount; i++)
        {
            var wi   = tmp.textInfo.wordInfo[i];
            string word = wi.GetWord();
            
            // 파라미터라면 (word가 숫자 하나)
            if (Regex.IsMatch(word, @"^\d+$"))
            {
                int paramIdx = int.Parse(word);

                // baseParams에서 이 파라미터가 치환되면 몇 단어를 차지하는지 계산
                int count = 1;
                if (baseParams != null && paramIdx < baseParams.Count)
                {
                    string paramValue = baseParams[paramIdx];
                    count = string.IsNullOrWhiteSpace(paramValue) ? 1 :
                        paramValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                }

                int start = i;
                int end = i + count - 1;
                paramRanges.Add(new ParamWordRange { start = start, end = end });
            }
        }

        GameObject.DestroyImmediate(canvasGo);
        return paramRanges;
    }
}
#endif
