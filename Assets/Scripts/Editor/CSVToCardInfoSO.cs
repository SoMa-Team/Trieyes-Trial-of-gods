#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CardSystem;

public class CSVToCardInfoSO : EditorWindow
{
    private string csvPath = "Assets/CardInfos/cardInfoData.csv";
    private string soOutputPath = "Assets/CardSystem/CardInfos/CardInfoSO";

    [MenuItem("Tools/CSVToStatPreset")]
    public static void ShowWindow()
    {
        GetWindow<CSVToCardInfoSO>("CSVToStatPreset");
    }

    void OnGUI()
    {
        GUILayout.Label("CSV 파일 경로", EditorStyles.boldLabel);
        csvPath = EditorGUILayout.TextField("CSV 파일 경로", csvPath);

        GUILayout.Label("SO 저장 경로", EditorStyles.boldLabel);
        soOutputPath = EditorGUILayout.TextField("SO 저장 경로", soOutputPath);

        if (GUILayout.Button("CSV로부터 CardInfoSO 생성/덮어쓰기"))
        {
            ImportCSVToSO();
        }
    }

    void ImportCSVToSO()
    {
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV 파일이 없습니다: {csvPath}");
            return;
        }

        if (!Directory.Exists(soOutputPath))
            Directory.CreateDirectory(soOutputPath);

        var lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("CSV 파일이 비어있거나 헤더만 있습니다.");
            return;
        }

        var headers = lines[0].Split(',');
        int idx_cardName = System.Array.IndexOf(headers, "cardName");
        int idx_rarity = System.Array.IndexOf(headers, "rarity");
        int idx_properties = System.Array.IndexOf(headers, "properties");
        int idx_illustration = System.Array.IndexOf(headers, "illustration");
        int idx_cardDescription = System.Array.IndexOf(headers, "cardDescription");
        int idx_eventTypes = System.Array.IndexOf(headers, "eventTypes");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var values = SplitCsvLine(lines[i]);
            if (values.Length < headers.Length) continue;

            string cardName = values[idx_cardName];

            // SO 파일 경로 생성
            string assetPath = Path.Combine(soOutputPath, $"{cardName}.asset");

            // 기존 SO가 있으면 불러오고, 없으면 새로 생성
            CardInfo card = AssetDatabase.LoadAssetAtPath<CardInfo>(assetPath);
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

            // eventTypes
            card.eventTypes = values[idx_eventTypes]
                .Split('|')
                .Select(s => (Utils.EventType)System.Enum.Parse(typeof(Utils.EventType), s))
                .ToList();

            if (isNew)
                AssetDatabase.CreateAsset(card, assetPath);
            else
                EditorUtility.SetDirty(card);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CardInfoSO 생성/덮어쓰기 완료!");
    }

    // 심플 CSV 파서
    string[] SplitCsvLine(string line)
    {
        return line.Split(',');
    }
}
#endif
