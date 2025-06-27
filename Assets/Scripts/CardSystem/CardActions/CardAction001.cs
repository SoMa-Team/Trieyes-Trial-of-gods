using UnityEngine;
using CharacterSystem;
using Stats;
using System.Collections.Generic;

namespace CardActions
{
    [System.Serializable]
    public class CardAction001 : CardAction
    {
        // ===== [기능 1] 상수 및 생성자 =====
        private const float DEFENSE_REDUCTION_PERCENT = 0.1f; // 10% 감소
        private const float ATTACK_INCREASE_PERCENT = 0.2f; // 20% 증가
        
        // ===== [기능 2] 이벤트 처리 =====
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param); // 부모 클래스의 OnEvent 호출

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                if (owner != null)
                    {
                    // AttackPower +10
                    var modifier = new StatModifier(10, BuffOperationType.Additive);
                    owner.statSheet[StatType.AttackPower].AddBuff(modifier);
                    Debug.Log($"<color=yellow>[CardAction001] Applied: ATK +10. New Value: {owner.statSheet[StatType.AttackPower].Value}</color>");
                    }
            }
        }

        private void ApplyStatModification(Pawn target)
        {
    
        }
    }
} 