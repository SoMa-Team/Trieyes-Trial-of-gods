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
        string csvPath = "Assets/Trieyes/Data/CardInfoData/CardInfoData.csv";
        string soOutputPath = "Assets/Trieyes/ScriptableObjects/CardInfos";

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
            string soName = values[idx_soName].Trim();
            string assetPath = Path.Combine(soOutputPath, $"{soName}.asset");

            CardInfo card = AssetDatabase.LoadAssetAtPath<CardInfo>(assetPath.Replace(Application.dataPath, "Assets"));
            bool isNew = false;
            if (card == null)
            {
                card = ScriptableObject.CreateInstance<CardInfo>();
                isNew = true;
            }
            
            card.Id = Id;
            card.cardName = cardName;

            if (System.Enum.TryParse(typeof(Rarity), values[idx_rarity], out object rarityValue))
                card.rarity = (Rarity)rarityValue;

            card.properties = values[idx_properties]
                .Split('|')
                .Select(s => (Property)System.Enum.Parse(typeof(Property), s))
                .ToArray();

            card.illustration = Resources.Load<Sprite>($"CardIllustration/{values[idx_illustration]}");
            if (card.illustration == null)
                Debug.LogWarning($"{cardName}: CardIllustration/{values[idx_illustration]} Sprite를 Resources에서 못 찾음!");

            card.cardDescription = values[idx_cardDescription].Replace("\\n", "\n");
            Debug.Log(card.cardDescription);

            card.eventTypes = values[idx_eventTypes]
                .Split('|')
                .Select(s => (Utils.EventType)System.Enum.Parse(typeof(Utils.EventType), s))
                .ToList();
            
            if (idx_baseParams >= 0 && !string.IsNullOrWhiteSpace(values[idx_baseParams]))
                card.baseParams = values[idx_baseParams].Split('|').Select(x => x.Trim()).ToList();
            else
                card.baseParams = new List<string>();
            
            card.paramCharRanges = ParseParamCharRangesWithTMP(card.cardDescription, card.baseParams);

            if (isNew)
                AssetDatabase.CreateAsset(card, assetPath.Replace(Application.dataPath, "Assets"));
            else
                EditorUtility.SetDirty(card);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CardInfoSO 생성/덮어쓰기 완료!");
    }

    /// <summary>
    /// 카드 설명(템플릿)에서 파라미터 치환 후, 각 파라미터 값의 [start, end] 글자 인덱스를 계산
    /// </summary>
    private static List<ParamCharRange> ParseParamCharRangesWithTMP(string description, List<string> baseParams)
    {
        // 1. 치환된 텍스트 생성
        string filled = description;
        for (int i = 0; i < baseParams.Count; i++)
            filled = filled.Replace("{" + i + "}", baseParams[i]);

        var ranges = new List<ParamCharRange>();
        int offset = 0;

        for (int i = 0; i < baseParams.Count; i++)
        {
            string paramValue = baseParams[i];
            int start = filled.IndexOf(paramValue, offset, StringComparison.Ordinal);
            if (start == -1)
            {
                Debug.LogWarning($"파라미터 값 '{paramValue}'를 치환된 텍스트에서 찾을 수 없습니다. 중복 값 등 확인 필요.");
                continue;
            }
            int end = start + paramValue.Length - 1;
            ranges.Add(new ParamCharRange { start = start, end = end });
            offset = end + 1;
        }

        return ranges;
    }
}
#endif
