using UnityEngine;
using System.Collections.Generic;

namespace CardSystem
{
    public class CardFactory : MonoBehaviour
    {
        // NOTE: 실제 게임에서는 카드 데이터를 ScriptableObject나 JSON 등으로 관리하고 로드하는 로직이 필요합니다.
        // 여기서는 예시를 위해 하드코딩된 데이터를 사용합니다.

        /// <summary>
        /// 주어진 카드 ID에 해당하는 Card 인스턴스를 생성하여 반환합니다.
        /// </summary>
        /// <param name="cardId">생성할 카드의 고유 ID</param>
        /// <returns>생성된 Card 인스턴스</returns>
        public Card GetCard(int cardId)
        {
            // CardActionFactory가 필요하므로 여기에 참조를 가져오거나 매개변수로 받아야 합니다.
            // 현재는 간단히 새로운 CardAction 인스턴스를 생성하는 것으로 가정합니다.
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
                        action: cardActionFactory.GetAction(1) // CardAction001에 해당
                    );
                case 2:
                    return new Card(
                        id: 2,
                        name: "Shield Card",
                        cardProperties: new Property[] { Property.Defense, Property.Health },
                        initialLevel: 1,
                        initialExp: 0,
                        rarity: Rarity.Uncommon,
                        action: cardActionFactory.GetAction(2) // CardAction002에 해당
                    );
                // ... 더 많은 카드 ID에 대한 케이스를 추가할 수 있습니다.
                default:
                    Debug.LogWarning($"Card with ID {cardId} not found.");
                    return null;
            }
        }
    }
} 