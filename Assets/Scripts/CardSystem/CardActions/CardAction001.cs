using UnityEngine;
using CharacterSystem;
using Stats;

namespace CardActions
{
    public class CardAction001 : CardAction
    {
        // ===== [기능 1] 상수 및 생성자 =====
        private const float DEFENSE_REDUCTION_PERCENT = 0.1f; // 10% 감소
        private const float ATTACK_INCREASE_PERCENT = 0.2f; // 20% 증가
        
        public CardAction001(int cardActionId) : base(cardActionId)
        {
            // 생성자, 필요에 따라 초기화 로직 추가
        }

        // ===== [기능 2] 이벤트 처리 =====
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            switch (eventType)
            {
                case Utils.EventType.OnBattleSceneChange:
                    break;

                case Utils.EventType.OnProjectileKill:
                    // 2. 적 처치 시: 공격력 1 증가
                    break;

                // 기타 이벤트별 동작 추가
            }
        }

        private void ApplyStatModification(Pawn target)
        {
            var defenseStat = target.GetStat(StatType.Defense);
            if (defenseStat != null)
            {
                int reduction = (int)(defenseStat.Value * DEFENSE_REDUCTION_PERCENT);
                target.ModifyStat(StatType.Defense, -reduction);
                Debug.Log($"<color=blue>CardAction001: {target.gameObject.name}의 방어력이 {reduction:F1} 감소했습니다.</color>");
            }
            var attackStat = target.GetStat(StatType.AttackPower);
            if (attackStat != null)
            {
                int increase = (int)(attackStat.Value * ATTACK_INCREASE_PERCENT);
                target.ModifyStat(StatType.AttackPower, increase);
                Debug.Log($"<color=red>CardAction001: {target.gameObject.name}의 공격력이 {increase:F1} 증가했습니다.</color>");
            }
        }
    }
} 