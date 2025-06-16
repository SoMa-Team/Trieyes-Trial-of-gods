using UnityEngine;
using CharacterSystem;
using Utils;

namespace CardSystem
{
    public class CardAction001 : CardAction
    {
        private const float DEFENSE_REDUCTION_PERCENT = 0.1f; // 10% 감소
        private const float ATTACK_INCREASE_PERCENT = 0.2f; // 20% 증가
        
        public CardAction001(int cardActionId) : base(cardActionId)
        {
            // 생성자, 필요에 따라 초기화 로직 추가
        }

        public override void OnEvent(Core.EventType eventType, object param)
        {
            switch (eventType)
            {
                case Core.EventType.OnBattleSceneChange:
                    if (param is Pawn targetPawn)
                    {
                        ApplyStatModification(targetPawn);
                    }
                    break;
            }
        }

        private void ApplyStatModification(Pawn target)
        {
            // 방어력 감소
            var defenseStat = target.GetStat(StatType.Defense);
            if (defenseStat != null)
            {
                int reduction = (int)(defenseStat.value * DEFENSE_REDUCTION_PERCENT);
                target.ModifyStat(StatType.Defense, -reduction);
                Debug.Log($"<color=blue>CardAction001: {target.gameObject.name}의 방어력이 {reduction:F1} 감소했습니다.</color>");
            }

            // 공격력 증가
            var attackStat = target.GetStat(StatType.AttackPower);
            if (attackStat != null)
            {
                int increase = (int)(attackStat.value * ATTACK_INCREASE_PERCENT);
                target.ModifyStat(StatType.AttackPower, increase);
                Debug.Log($"<color=red>CardAction001: {target.gameObject.name}의 공격력이 {increase:F1} 증가했습니다.</color>");
            }
        }
    }
} 