using UnityEngine;
using System.Collections.Generic;
using CardActions;

namespace CardSystem
{
    public class CardFactory : MonoBehaviour
    {
        /// <summary>
        /// 주어진 카드 ID에 해당하는 Card 인스턴스를 생성하여 반환합니다.
        /// </summary>
        /// <param name="cardId">생성할 카드의 고유 ID</param>
        /// <returns>생성된 Card 인스턴스</returns>
        public Card GetCard(int cardId)
        {
            // ===== [기능 1] Card 생성 =====
            CardActionFactory cardActionFactory = GetComponent<CardActionFactory>();
            if (cardActionFactory == null)
            {
                Debug.LogError("CardActionFactory not found on this GameObject.");
                return null;
            }
            switch (cardId)
            {
                case 1:
                    return new Card(
                        id: 1,
                        name: "Fireball Card",
                        cardProperties: new Property[] { Property.Attack, Property.CritDamage },
                        initialLevel: 1,
                        initialExp: 0,
                        rarity: Rarity.Common,
                        action: cardActionFactory.GetAction(1)
                    );
                case 2:
                    return new Card(
                        id: 2,
                        name: "Shield Card",
                        cardProperties: new Property[] { Property.Defense, Property.Health },
                        initialLevel: 1,
                        initialExp: 0,
                        rarity: Rarity.Uncommon,
                        action: cardActionFactory.GetAction(2)
                    );
                // ... 더 많은 카드 ID에 대한 케이스를 추가할 수 있습니다.
                default:
                    Debug.LogWarning($"Card with ID {cardId} not found.");
                    return null;
            }
        }
    }
} 