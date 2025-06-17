using CardActions;
using Utils; // For EventType
using UnityEngine; // For Debug.LogWarning

namespace CardSystem
{
    // --- CardActionFactory 클래스 ---
    /// <summary>
    /// 다양한 CardAction 인스턴스를 생성하는 팩토리 클래스입니다.
    /// 각 CardAction은 특정 게임 이벤트에 반응하는 고유한 로직을 캡슐화합니다.
    /// </summary>
    public class CardActionFactory : MonoBehaviour
    {
        // ===== [기능 1] CardAction 생성 =====
        /// <summary>
        /// 주어진 카드 액션 ID에 해당하는 CardAction 인스턴스를 생성하여 반환합니다.
        /// 이 메서드는 게임 내에서 특정 카드 액션이 필요할 때 호출됩니다.
        /// </summary>
        /// <param name="cardActionId">생성할 카드 액션의 고유 ID</param>
        /// <returns>생성된 CardAction 인스턴스 (ID에 해당하는 액션이 없으면 null 반환)</returns>
        public CardAction GetAction(int cardActionId)
        {
            switch (cardActionId)
            {
                case 1:
                    return new CardAction001(cardActionId);
                default:
                    Debug.LogWarning($"CardAction with ID {cardActionId} not found.");
                    return null;
            }
        }
    }
} 