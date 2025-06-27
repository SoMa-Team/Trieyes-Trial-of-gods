using System.Collections.Generic;
using Utils; // For IEventHandler and EventType
using UnityEngine;
using CharacterSystem;
using CardSystem;

namespace CardActions
{
    /// <summary>
    /// 카드 액션의 기본 동작을 정의하는 추상 클래스입니다.
    /// 모든 구체적인 카드 액션은 이 클래스를 상속받아 고유한 OnEvent 로직을 구현해야 합니다.
    /// 카드의 특별한 효과와 게임 로직을 담당하는 핵심 클래스입니다.
    /// </summary>
    public abstract class CardAction
    {
        /// <summary>
        /// 카드 액션이 특정 이벤트에 반응할 때 호출되는 가상 메서드입니다.
        /// 하위 클래스에서 오버라이드하여 구체적인 카드 효과를 구현합니다.
        /// 게임 이벤트에 따라 카드의 특별한 능력을 발동시킵니다.
        /// </summary>
        /// <param name="owner">카드를 소유한 캐릭터</param>
        /// <param name="deck">카드가 속한 덱</param>
        /// <param name="eventType">발생한 이벤트 타입</param>
        /// <param name="param">이벤트와 함께 전달된 매개변수</param>
        public virtual void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            // 기본 구현은 비어있습니다.
            // 하위 클래스에서 오버라이드하여 구체적인 로직을 구현합니다.
        }
    }
} 