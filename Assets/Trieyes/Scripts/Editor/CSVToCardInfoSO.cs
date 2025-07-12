#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CardSystem;
using Utils;

public static class CSVToCardInfoSOImporter
{
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
        int idx_soName = System.Array.IndexOf(headers, "soName");
        int idx_cardName = System.Array.IndexOf(headers, "cardName");
        int idx_rarity = System.Array.IndexOf(headers, "rarity");
        int idx_properties = System.Array.IndexOf(headers, "properties");
        int idx_illustration = System.Array.IndexOf(headers, "illustration");
        int idx_cardDescription = System.Array.IndexOf(headers, "cardDescription");
        int idx_eventTypes = System.Array.IndexOf(headers, "eventTypes");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var values = CsvUtils.SplitCsvLine(lines[i]).ToArray();

            if (values.Length < headers.Length) continue;

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
            card.illustration = Resources.Load<Sprite>(values[idx_illustration]);
            if (card.illustration == null)
                Debug.LogWarning($"{cardName}: {values[idx_illustration]} Sprite를 Resources에서 못 찾음!");

            card.cardDescription = values[idx_cardDescription];
            Debug.Log(card.cardDescription);

            // eventTypes
            card.eventTypes = values[idx_eventTypes]
                .Split('|')
                .Select(s => (Utils.EventType)System.Enum.Parse(typeof(Utils.EventType), s))
                .ToList();

            if (isNew)
                AssetDatabase.CreateAsset(card, assetPath.Replace(Application.dataPath, "Assets"));
            else
                EditorUtility.SetDirty(card);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CardInfoSO 생성/덮어쓰기 완료!");
    }
}
#endif
