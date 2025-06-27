using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardActions;

namespace CardSystem
{
    /// <summary>
    /// 카드 생성을 담당하는 팩토리 클래스입니다.
    /// 싱글톤 패턴을 사용하여 카드의 생성과 활성화/비활성화를 관리합니다.
    /// </summary>
    public class CardFactory : MonoBehaviour
    {
        // --- 필드 ---

        /// <summary>
        /// CardFactory의 싱글톤 인스턴스입니다.
        /// </summary>
        public static CardFactory Instance { private set; get; }
        public List<CardInfo> cardInfos = new List<CardInfo>();

        // --- private 메서드 ---

        /// <summary>
        /// MonoBehaviour의 Awake 메서드입니다.
        /// 싱글톤 패턴을 구현하여 중복 인스턴스를 방지합니다.
        /// </summary>
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // --- public 메서드 ---

        /// <summary>
        /// 새로운 카드를 생성하고 활성화합니다.
        /// </summary>
        /// <param name="level">카드의 초기 레벨</param>
        /// <param name="CardActionID">생성할 CardAction의 ID</param>
        /// <returns>생성된 카드 객체</returns>
        public Card Create(int level, int CardActionID)
        {
            Card card = new Card();
            Activate(card, level, CardActionID);
            return card;
        }

        /// <summary>
        /// 기존 카드를 활성화합니다.
        /// 카드의 CardAction을 설정하고 스탯을 초기화합니다.
        /// </summary>
        /// <param name="card">활성화할 카드</param>
        /// <param name="level">카드의 레벨</param>
        /// <param name="CardActionID">설정할 CardAction의 ID</param>
        private void Activate(Card card, int level, int CardActionID)
        {
            switch(CardActionID)
            {
                case 0:
                    InitCardInfo(card, cardInfos[CardActionID]);
                    card.cardAction = new PreparingMarch();
                    break;
                case 1:
                    InitCardInfo(card, cardInfos[CardActionID]);
                    card.cardAction = new Crouch();
                    break;
                case 2:
                    InitCardInfo(card, cardInfos[CardActionID]);
                    card.cardAction = new Shadow();
                    break;
                default:
                    break;
            }
            card?.Activate();
        }
        
        private void InitCardInfo(Card card, CardInfo info)
        {
            card.cardName = info.cardName;
            card.rarity = info.rarity;
            card.properties = info.properties;
            card.illustration = info.illustration;
            card.cardDescription = info.cardDescription;
            card.eventTypes = new List<Utils.EventType>(info.eventTypes); // 깊은 복사
        }

        /// <summary>
        /// 카드를 비활성화합니다.
        /// </summary>
        /// <param name="card">비활성화할 카드</param>
        public void Deactivate(Card card)
        {
            card.Deactivate();
        }
    }
} 