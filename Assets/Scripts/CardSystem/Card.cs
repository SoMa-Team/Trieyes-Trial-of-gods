using System.Collections.Generic;
using Stats;
using CardActions;

namespace CardSystem
{
    public class Card
    {
        // ===== [기능 1] 카드 정보 및 생성 =====
        public int cardId { get; private set; }
        public string cardName { get; private set; }
        public Property[] Properties { get; private set; }
        public Rarity cardRarity { get; private set; }
        public CardEnhancement cardEnhancement;
        public CardStat[] cardStats;
        public CardAction cardAction;

        /// <summary>
        /// card 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="id">카드의 고유 ID</param>
        /// <param name="name">카드의 이름</param>
        /// <param name="cardProperties">카드가 가지는 속성들</param>
        /// <param name="initialLevel">카드의 초기 레벨</param>
        /// <param name="initialExp">카드의 초기 경험치</param>
        /// <param name="rarity">카드의 희귀도</param>
        /// <param name="action">카드와 연관된 행동 (cardAction 인스턴스)</param>
        public Card(int id, string name, Property[] cardProperties, int initialLevel, int initialExp, Rarity rarity, CardAction action)
        {
            cardId = id;
            cardName = name;
            Properties = cardProperties;
            cardRarity = rarity;
            cardAction = action;
            cardEnhancement = new CardEnhancement(initialLevel, initialExp);
            InitializecardStats();
        }

        /// <summary>
        /// cardEnhancement의 레벨을 기반으로 cardStat들을 초기화합니다.
        /// </summary>
        private void InitializecardStats()
        {
            List<CardStat> stats = new List<CardStat>();
            foreach (var prop in Properties)
            {
                stats.Add(new CardStat(prop, cardEnhancement.level));
            }
            cardStats = stats.ToArray();
        }

        // ===== [기능 2] 카드 스탯 관련 =====
        /// <summary>
        /// 이 카드가 가진 모든 스탯 정보를 합산하여 반환합니다.
        /// </summary>
        /// <returns>합산된 StatInfo 리스트</returns>
        public List<StatInfo> getAllCardStatInfos()
        {
            List<StatInfo> allStats = new List<StatInfo>();
            foreach (var cardStat in cardStats)
            {
                if (cardStat?.StatInfos != null)
                {
                    allStats = StatCalculator.AddStats(allStats, cardStat.StatInfos);
                }
            }
            return allStats;
        }

        // ===== [기능 3] 카드 액션 이벤트 트리거 =====
        /// <summary>
        /// 이 카드의 cardAction에 특정 이벤트를 발동시킵니다.
        /// 덱(Deck)에서 이벤트를 전파할 때 이 메서드를 호출하여 해당 카드 액션의 OnEvent를 실행합니다.
        /// </summary>
        /// <param name="eventType">발동시킬 이벤트의 타입</param>
        /// <param name="param">이벤트와 함께 전달될 추가 데이터 (선택 사항)</param>
        public void TriggerCardEvent(Utils.EventType eventType, object param = null)
        {
            cardAction?.OnEvent(eventType, param);
        }
    }
} 