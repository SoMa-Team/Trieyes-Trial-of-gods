using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardActions;
using System;
using EventType = Utils.EventType;

namespace CardSystem
{
    /// <summary>
    /// 카드 생성과 초기화/비활성화를 담당하는 팩토리(싱글톤) 클래스.
    /// CardInfo 기반 데이터와 CardAction을 연결해 새로운 카드를 생성합니다.
    /// </summary>
    public class CardFactory : MonoBehaviour
    {
        // ==== [필드] ====

        /// <summary>
        /// CardFactory 싱글톤 인스턴스
        /// </summary>
        public static CardFactory Instance { get; private set; }

        /// <summary>
        /// 등록된 CardInfo ScriptableObject 리스트.
        /// 인덱스가 CardActionID 역할도 겸함.
        /// </summary>
        public List<CardInfo> cardInfos = new();

        // ==== [싱글톤 패턴] ====

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // (DontDestroyOnLoad(gameObject)); // 씬 이동 시에도 유지하려면 주석 해제
        }

        // ==== [카드 생성 및 관리] ====

        /// <summary>
        /// 새로운 카드를 생성 및 초기화합니다.
        /// </summary>
        /// <param name="level">생성할 카드 레벨</param>
        /// <param name="CardActionID">생성할 CardInfo/CardAction 인덱스</param>
        public Card Create(int level, int CardActionID)
        {
            Debug.Log($"[CardFactory] Create Card: {CardActionID}, Level: {level}");
            var card = new Card();
            Activate(card, level, CardActionID);
            return card;
        }

        /// <summary>
        /// 카드 비활성화 (자원 정리용)
        /// </summary>
        public void Deactivate(Card card)
        {
            card.Deactivate();
        }

        // ==== [내부 - 카드 데이터 세팅] ====

        /// <summary>
        /// 카드에 CardInfo 및 CardAction을 할당하고, 레벨에 맞게 초기화합니다.
        /// </summary>
        private void Activate(Card card, int level, int CardActionID)
        {
            if (CardActionID < 0 || CardActionID >= cardInfos.Count)
            {
                Debug.LogError($"[CardFactory] 유효하지 않은 CardActionID: {CardActionID}");
                return;
            }
            InitCardInfo(card, cardInfos[CardActionID]);
            var action = CreateActionByID(CardActionID);

            if (action != null)
            {
                card.SetCardAction(action);
                action.SetCard(card);
            }

            card.Activate(level);
        }

        /// <summary>
        /// CardActionID에 따라 액션 객체를 생성
        /// </summary>
        private CardAction CreateActionByID(int id)
        {
            switch (id)
            {
                case 0: return new Card0001_PreparingMarch();
                case 1: return new Card0002_Crouch();
                case 2: return new Card0003_Haste();
                case 3: return new Card0004_WeightReduction();
                case 4: return new Card0201_ImmatureSparring();
                case 5: return new Card0202_WeaponEnlargement();
                case 6: return new Card0401_Shadow();
                case 7: return new Card0402_WeightOfArmor();
                case 8: return new Card0601_AbsorbingBlade();
                default:
                    Debug.LogWarning($"[CardFactory] 지원하지 않는 CardActionID: {id}");
                    return null;
            }
        }

        /// <summary>
        /// 카드에 CardInfo 데이터를 복사(초기화)합니다.
        /// </summary>
        private void InitCardInfo(Card card, CardInfo info)
        {
            card.cardName = info.cardName;
            card.rarity = info.rarity;
            card.properties = info.properties;
            card.illustration = info.illustration;
            card.cardDescription = info.cardDescription;
            card.eventTypes = info.eventTypes != null ? new List<EventType>(info.eventTypes) : new List<EventType>();
            card.baseParams = info.baseParams != null ? new List<string>(info.baseParams) : new List<string>();
            card.paramWordRanges = info.paramWordRanges != null ? new List<ParamWordRange>(info.paramWordRanges) : new List<ParamWordRange>();
        }
    }
}
