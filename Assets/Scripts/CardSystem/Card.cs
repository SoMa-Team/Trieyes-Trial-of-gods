using UnityEngine;
using System.Collections.Generic;
using Stats;
using CardActions;
using System;
using CharacterSystem;

namespace CardSystem
{
    /// <summary>
    /// 카드의 고유 식별자를 위한 타입 별칭입니다.
    /// </summary>
    using CardID = Int32;

    /// <summary>
    /// 카드 시스템의 핵심 클래스입니다.
    /// 카드의 기본 정보, 액션, 스탯, 강화 정보를 관리합니다.
    /// CardInfo와 CardAction을 분리하여 데이터와 로직을 명확히 구분합니다.
    /// </summary>
    public class Card
    {
        // --- 필드 ---

        /// <summary>
        /// 카드 ID 생성을 위한 정적 카운터입니다.
        /// 새로운 카드가 생성될 때마다 자동으로 증가합니다.
        /// </summary>
        private static int idCounter = 0;

        [Header("Card Info")]
        /// <summary>
        /// 카드의 고유 식별자입니다.
        /// </summary>
        public CardID cardId;

        /// <summary>
        /// 카드가 가진 속성들의 배열입니다.
        /// 공격, 방어, 체력 등의 속성을 정의합니다.
        /// </summary>
        public Property[] properties;

        /// <summary>
        /// 카드의 희귀도입니다.
        /// Common, Uncommon, Rare, Epic, Legendary 중 하나입니다.
        /// </summary>
        public Rarity rarity;

        /// <summary>
        /// 카드의 이름입니다.
        /// UI에서 표시되는 카드의 제목입니다.
        /// </summary>
        public string cardName;

        /// <summary>
        /// 카드의 일러스트레이션 이미지입니다.
        /// UI에서 카드를 시각적으로 표현하는 데 사용됩니다.
        /// </summary>
        public Sprite illustration;

        /// <summary>
        /// 카드의 설명 텍스트입니다.
        /// UI에서 카드의 효과를 설명하는 데 사용됩니다.
        /// </summary>
        [TextArea] public string cardDescription;

        /// <summary>
        /// 해당 카드가 반응하는 이벤트 타입들의 리스트입니다.
        /// 이벤트 처리 최적화를 위해 사용됩니다.
        /// </summary>
        public List<Utils.EventType> eventTypes = new();

        /// <summary>
        /// 카드의 액션을 정의하는 CardAction 객체입니다.
        /// 카드의 특별한 효과와 로직을 담당합니다.
        /// </summary>
        public CardAction cardAction;

        /// <summary>
        /// 카드의 스탯 정보를 관리하는 객체입니다.
        /// 카드의 속성과 레벨에 따른 스탯을 계산합니다.
        /// </summary>
        public CardStat cardStats;

        /// <summary>
        /// 카드의 강화 정보(레벨, 경험치)를 관리하는 객체입니다.
        /// 카드의 레벨업과 경험치 시스템을 담당합니다.
        /// </summary>
        public CardEnhancement cardEnhancement;

        /// <summary>
        /// 카드를 소유한 캐릭터(Pawn)입니다.
        /// 카드의 효과가 적용될 대상입니다.
        /// </summary>
        private Pawn owner;

        // --- 생성자 ---

        /// <summary>
        /// Card의 새 인스턴스를 초기화합니다.
        /// 자동으로 고유한 cardId를 할당합니다.
        /// </summary>
        public Card()
        {
            this.cardId = idCounter++;
        }

        // --- public 메서드 ---

        /// <summary>
        /// 카드를 활성화하고 초기화합니다.
        /// 스탯과 강화 정보를 설정하여 카드를 사용 가능한 상태로 만듭니다.
        /// </summary>
        /// <param name="level">카드의 초기 레벨</param>
        public void Activate(int level)
        {
            Debug.Log($"Card Activated! {cardId}, card level: {level}");
            // 스탯과 강화 정보 초기화
            cardStats = new CardStat(properties, level);
            cardEnhancement = new CardEnhancement(level, 0);
        }

        /// <summary>
        /// 카드를 비활성화합니다.
        /// 현재는 구현되지 않았습니다.
        /// </summary>
        public void Deactivate()
        {
            // TODO: 카드 비활성화 로직 구현
            // - 리소스 해제
            // - 이벤트 구독 해제
            // - 상태 초기화
        }

        /// <summary>
        /// 카드 이벤트를 트리거합니다.
        /// CardAction의 OnEvent 메서드를 호출하여 카드의 특별한 효과를 실행합니다.
        /// </summary>
        /// <param name="eventType">발생한 이벤트 타입</param>
        /// <param name="deck">카드가 속한 덱</param>
        /// <param name="param">이벤트와 함께 전달될 추가 매개변수 (선택사항)</param>
        public void TriggerCardEvent(Utils.EventType eventType, CardSystem.Deck deck, object param = null)
        {
            cardAction.OnEvent(owner, deck, eventType, param);
        }

        /// <summary>
        /// 카드의 소유자를 설정합니다.
        /// 카드의 효과가 적용될 캐릭터를 지정합니다.
        /// </summary>
        /// <param name="pawn">카드를 소유할 캐릭터</param>
        public void SetOwner(CharacterSystem.Pawn pawn)
        {
            owner = pawn;
        }

        public void LevelUp()
        {
            cardEnhancement.level.AddToBasicValue(1);
            RefreshStats();
        }

        public void RefreshStats()
        {
            // 현재 property와 레벨로 CardStat 새로 생성
            cardStats = new CardStat(properties, cardEnhancement.level.Value);
        }

        
        public Card DeepCopy()
        {
            var clone = new Card();
            // 필드들 복사
            clone.properties = (Property[])this.properties.Clone();
            clone.rarity = this.rarity;
            clone.cardName = this.cardName;
            clone.illustration = this.illustration;
            clone.cardDescription = this.cardDescription;
            clone.eventTypes = new List<Utils.EventType>(this.eventTypes);

            // 내부 참조 타입 멤버들도 DeepCopy!
            clone.cardAction = this.cardAction?.DeepCopy();
            clone.cardStats = this.cardStats?.DeepCopy();
            clone.cardEnhancement = this.cardEnhancement?.DeepCopy();

            return clone;
        }
    }
} 