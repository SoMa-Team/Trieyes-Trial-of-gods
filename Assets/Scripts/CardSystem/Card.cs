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
    /// </summary>
    public class Card
    {
        // --- 필드 ---

        /// <summary>
        /// 카드 ID 생성을 위한 정적 카운터입니다.
        /// </summary>
        private static int idCounter = 0;

        [Header("Card Info")]
        /// <summary>
        /// 카드의 고유 식별자입니다.
        /// </summary>
        public CardID cardId;

        /// <summary>
        /// 카드의 액션을 정의하는 ScriptableObject 참조입니다.
        /// </summary>
        public CardAction cardActionSO;

        /// <summary>
        /// 카드의 스탯 정보를 관리하는 객체입니다.
        /// </summary>
        public CardStat cardStats;

        /// <summary>
        /// 카드의 강화 정보(레벨, 경험치)를 관리하는 객체입니다.
        /// </summary>
        public CardEnhancement cardEnhancement;

        /// <summary>
        /// 카드를 소유한 캐릭터(Pawn)입니다.
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
        /// CardActionFactory를 통해 CardAction을 생성하고, 스탯과 강화 정보를 설정합니다.
        /// </summary>
        /// <param name="level">카드의 초기 레벨</param>
        /// <param name="CardActionID">생성할 CardAction의 ID</param>
        /// <param name="owner">카드를 소유할 캐릭터 (선택사항)</param>
        public void Activate(int level, int CardActionID, Pawn owner = null)
        {
            // CardActionFactory가 초기화되었는지 확인
            if (CardActionFactory.Instance == null)
            {
                Debug.LogWarning("CardActionFactory가 초기화되지 않았습니다.");
                return;
            }

            // CardAction 생성
            cardActionSO = CardActionFactory.Instance.Create(CardActionID);
            if (cardActionSO == null)
            {
                Debug.LogWarning($"CardAction 생성 실패! CardActionID={CardActionID}");
                return;
            }

            // 스탯과 강화 정보 초기화
            cardStats = new CardStat(cardActionSO.properties, level);
            cardEnhancement = new CardEnhancement(level, 0);
            SetOwner(owner);
        }

        /// <summary>
        /// 카드를 비활성화합니다.
        /// 현재는 구현되지 않았습니다.
        /// </summary>
        public void Deactivate()
        {
            // TODO: 카드 비활성화 로직 구현
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
            cardActionSO?.OnEvent(owner, deck, eventType, param);
        }

        /// <summary>
        /// 카드의 소유자를 설정합니다.
        /// </summary>
        /// <param name="pawn">카드를 소유할 캐릭터</param>
        public void SetOwner(CharacterSystem.Pawn pawn)
        {
            owner = pawn;
        }
    }
} 