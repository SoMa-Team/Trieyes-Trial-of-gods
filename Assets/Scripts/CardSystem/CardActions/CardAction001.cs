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
            
        }

        private void ApplyStatModification(Pawn target)
        {
    
        }
    }
} 