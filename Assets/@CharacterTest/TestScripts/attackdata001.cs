using UnityEngine;
using AttackSystem;
using Stats;

[CreateAssetMenu(fileName = "attackdata001", menuName = "Attack/AttackData001", order = 1)]
public class attackdata001 : AttackData
{
    private void OnEnable()
    {
        attackId = 1;
        attackName = "기본공격001";
        attackType = AttackType.Basic;
        // cooldown은 스탯의 공격 속도와 같게, 런타임에 Pawn의 StatSheet에서 동기화 필요
        cooldown = 0f; // 런타임에 동기화 필요
        bIsActivated = false;
        // statSheet는 캐릭터001의 스탯과 일치시키게, 런타임에 동기화 필요
        // statSheet = ... (런타임에 캐릭터001의 StatSheet를 복사)
    }
} 