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

        private readonly float COMMON_PROB = 53f;
        private readonly float UNCOMMON_PROB = 33f;
        private readonly float LEGENDARY_PROB = 13f;
        private readonly float EXCEED_PROB = 1f;

        /// <summary>
        /// 등록된 CardInfo ScriptableObject 리스트.
        /// 인덱스가 CardActionID 역할도 겸함.
        /// </summary>
        [SerializeField] private List<CardInfo> RawCards;
        
        private Dictionary<int, CardInfo> cardDictionary = new Dictionary<int, CardInfo>();
        private List<int> CommonCards = new();
        private List<int> UncommonCards = new();
        private List<int> LegendaryCards = new();
        private List<int> ExceedCards = new();
        private List<int> GimmickCards = new();

        // ==== [싱글톤 패턴] ====

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동 시에도 유지하려면 주석 해제
        }

        private void Start()
        {
            foreach (var cardInfo in RawCards)
            {
                cardDictionary.Add(cardInfo.Id, cardInfo);

                (cardInfo.rarity switch
                {
                    Rarity.Common => CommonCards,
                    Rarity.Uncommon => UncommonCards,
                    Rarity.Legendary => LegendaryCards,
                    Rarity.Exceed => ExceedCards,
                    Rarity.Gimmick => GimmickCards,
                    _ => throw new ArgumentOutOfRangeException()
                }).Add(cardInfo.Id);
            }
        }

        // ==== [카드 생성 및 관리] ====

        /// <summary>
        /// 새로운 카드를 생성 및 초기화합니다.
        /// </summary>
        /// <param name="level">생성할 카드 레벨</param>
        /// <param name="CardActionID">생성할 CardInfo/CardAction 인덱스</param>
        public Card Create(int level, CardInfo cardInfo)
        {
            var card = new Card();
            Activate(card, level, cardInfo);
            return card;
        }

        public Card RandomCreate(int level = 1)
        {
            // 누적 확률 (0~100)
            // Common: 53%, Uncommon: 33%, Legendary: 13%, Exceed: 1%
            float rand = UnityEngine.Random.Range(0f, 100f);

            List<int> pool;
            
            if (rand < COMMON_PROB) // Common
            {
                pool = CommonCards;
            }
            else if (rand < COMMON_PROB+UNCOMMON_PROB) // Uncommon
            {
                pool = UncommonCards;
            }
            else if (rand < COMMON_PROB+UNCOMMON_PROB+LEGENDARY_PROB) // Legendary
            {
                pool = LegendaryCards;
            }
            else // Exceed
            {
                pool = ExceedCards;
            }

            if (pool == null || pool.Count == 0)
            {
                Debug.LogWarning("[CardFactory] 카드 풀에 카드가 없습니다.");
                return null;
            }

            int idx = UnityEngine.Random.Range(0, pool.Count);
            int cardID = pool[idx];

            return CreateByID(cardID);
        }

        public Card CreateByID(int id, int level = 1)
        {
            if (!cardDictionary.ContainsKey(id))
                return null;
            
            var cardInfo = cardDictionary[id];
            return Create(level, cardInfo);
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
        private void Activate(Card card, int level, CardInfo cardInfo)
        {
            InitCardInfo(card, cardInfo);
            var action = CreateActionByID(cardInfo.Id);

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
                case 1: return new Card0001_PreparingMarch();
                case 2: return new Card0002_Crouch();
                case 3: return new Card0003_Haste();
                case 4: return new Card0004_WeightReduction();
                case 5: return new Card0005_Roll();
                case 201: return new Card0201_ImmatureSparring();
                case 202: return new Card0202_WeaponEnlargement();
                case 401: return new Card0401_Shadow();
                case 402: return new Card0402_WeightOfArmor();
                case 403: return new Card0403_AttackIsTheBestDefense();
                case 404: return new Card0404_KillingSpree();
                case 601: return new Card0601_AbsorbingBlade();
                case 602: return new Card0602_Berserker();
                case 801: return new Card0801_FTL();
                case 802: return new Card0802_RageOfBlade();
                case 1001: return new Card2001_Stop();
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
            card.paramCharRanges = info.paramCharRanges != null ? new List<ParamCharRange>(info.paramCharRanges) : new List<ParamCharRange>();
        }
    }
}
