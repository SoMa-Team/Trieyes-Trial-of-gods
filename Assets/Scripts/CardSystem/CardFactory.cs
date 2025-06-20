using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardActions;

namespace CardSystem
{
    public class CardFactory : MonoBehaviour
    {
        [Header("Card Prefabs")]
        [SerializeField] private List<Card> cardPrefabs = new List<Card>();

        /// <summary>
        /// 주어진 카드 ID에 해당하는 Card 인스턴스를 생성하여 반환합니다.
        /// </summary>
        /// <param name="cardId">생성할 카드의 고유 ID</param>
        /// <returns>생성된 Card 인스턴스</returns>
        public Card GetCard(int cardId)
        {
            Card prefab = cardPrefabs.FirstOrDefault(p => p.cardId == cardId);

            if (prefab != null)
            {
                // 프리팹을 복제하여 새 인스턴스를 만듭니다.
                return Instantiate(prefab);
            }
            else
            {
                Debug.LogWarning($"Card prefab with ID {cardId} not found in factory.");
                return null;
            }
        }
    }
} 