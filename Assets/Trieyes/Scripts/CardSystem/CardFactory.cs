using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardActions;

namespace CardSystem
{
    /// <summary>
    /// 카드 생성을 담당하는 팩토리 클래스입니다.
    /// 싱글톤 패턴을 사용하여 카드의 생성과 활성화/비활성화를 관리합니다.
    /// CardInfo를 기반으로 카드를 생성하고 CardAction을 설정합니다.
    /// </summary>
    public class CardFactory : MonoBehaviour
    {
        // --- 필드 ---

        /// <summary>
        /// CardFactory의 싱글톤 인스턴스입니다.
        /// </summary>
        public static CardFactory Instance { private set; get; }

        /// <summary>
        /// 등록된 모든 CardInfo ScriptableObject들의 리스트입니다.
        /// 인덱스가 CardActionID로 사용되며, 카드 생성 시 참조됩니다.
        /// </summary>
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
        /// CardActionID에 따라 적절한 CardAction을 생성하고 카드 정보를 설정합니다.
        /// </summary>
        /// <param name="level">카드의 초기 레벨</param>
        /// <param name="CardActionID">생성할 CardAction의 ID</param>
        /// <returns>생성된 카드 객체</returns>
        public Card Create(int level, int CardActionID)
        {
            Debug.Log($"Creating Card {CardActionID}, Card Level: {level}");
            Card card = new Card();
            Activate(card, level, CardActionID);
            return card;
        }

        /// <summary>
        /// 카드를 비활성화합니다.
        /// </summary>
        /// <param name="card">비활성화할 카드</param>
        public void Deactivate(Card card)
        {
            card.Deactivate();
        }

        // --- private 메서드 ---

        /// <summary>
        /// 기존 카드를 활성화합니다.
        /// CardActionID에 따라 적절한 CardAction을 설정하고 카드 정보를 초기화합니다.
        /// </summary>
        /// <param name="card">활성화할 카드</param>
        /// <param name="level">카드의 레벨</param>
        /// <param name="CardActionID">설정할 CardAction의 ID</param>
        private void Activate(Card card, int level, int CardActionID)
        {
            InitCardInfo(card, cardInfos[CardActionID]);
            CardAction action = null;

            switch(CardActionID)
            {
                case 0:
                    action = new PreparingMarch();
                    break;
                case 1:
                    action = new Crouch();
                    break;
                case 2:
                    action = new Shadow();
                    break;
                default:
                    Debug.LogWarning($"[CardFactory] 지원하지 않는 CardActionID: {CardActionID}");
                    break;
            }
            // 카드와 액션 연결
            if (action != null)
                card.SetCardAction(action);

            card?.Activate(level);
        }

        /// <summary>
        /// 카드에 CardInfo의 정보를 복사합니다.
        /// 카드의 기본 정보(이름, 희귀도, 속성, 이미지, 설명, 이벤트 타입)를 설정합니다.
        /// </summary>
        /// <param name="card">정보를 설정할 카드</param>
        /// <param name="info">복사할 CardInfo 객체</param>
        private void InitCardInfo(Card card, CardInfo info)
        {
            card.cardName = info.cardName;
            card.rarity = info.rarity;
            card.properties = info.properties;
            card.illustration = info.illustration;
            card.cardDescription = info.cardDescription;
            card.eventTypes = new List<Utils.EventType>(info.eventTypes);
            card.baseParams = info.baseParams != null
                ? new List<string>(info.baseParams)
                : new List<string>();
        }
    }
} 