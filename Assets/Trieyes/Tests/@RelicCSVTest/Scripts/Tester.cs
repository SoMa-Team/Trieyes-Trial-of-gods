using UnityEngine;
using RelicSystem;

public class Tester : MonoBehaviour
{
    async void Start()
    {
        await RelicDataBase.InitializeAsync();

        for (int i = 0; i < 4; i++)
        {
            RelicDataSO data = RelicDataBase.GetRelicDataSO(i);
            if (data == null)
            {
                Debug.LogWarning($"[Relic] id {i}: SO가 존재하지 않습니다.");
                continue;
            }

            Debug.Log($"========== Relic [{data.id}] ==========");
            Debug.Log($"  이름: {data.name}");
            Debug.Log($"  설명: {data.description}");

            // attackComponentIDs
            if (data.attackComponentIDs != null && data.attackComponentIDs.Count > 0)
            {
                Debug.Log($"  attackComponentIDs: {string.Join(", ", data.attackComponentIDs)}");
            }
            else
            {
                Debug.Log($"  attackComponentIDs: (없음)");
            }

            // filterAttackIDs
            if (data.filterAttackIDs != null && data.filterAttackIDs.Count > 0)
            {
                Debug.Log($"  filterAttackIDs: {string.Join(", ", data.filterAttackIDs)}");
            }
            else if (data.filterAttackIDs == null)
            {
                Debug.Log($"  filterAttackIDs: null");
            }
            else
            {
                Debug.Log($"  filterAttackIDs: (빈 배열)");
            }

            // filterTag
            if (!string.IsNullOrEmpty(data.filterTag))
            {
                Debug.Log($"  filterTag: {data.filterTag}");
            }
            else
            {
                Debug.Log($"  filterTag: null");
            }

            Debug.Log("====================================");
        }
    }
}