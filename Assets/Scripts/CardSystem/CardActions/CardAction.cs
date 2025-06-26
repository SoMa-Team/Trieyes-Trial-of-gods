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
    /// ScriptableObject를 상속받아 Unity 에디터에서 설정 가능합니다.
    /// </summary>
    public abstract class CardAction : ScriptableObject
    {
        [Header("Action Info")]
        /// <summary>
        /// 이 카드 액션이 가진 속성들의 배열입니다.
        /// 카드의 기본 스탯을 결정하는 데 사용됩니다.
        /// </summary>
        public Property[] properties;
        /// <summary>
        /// 카드의 희귀도입니다.
        /// 카드의 등급과 강화 가능성을 결정합니다.
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
        /// 해당 카드 액션이 반응하는 이벤트 타입들의 리스트입니다.
        /// 이벤트 처리 최적화를 위해 사용됩니다.
        /// </summary>
        public List<Utils.EventType> eventTypes = new();

        /// <summary>
        /// 카드 액션이 특정 이벤트에 반응할 때 호출되는 가상 메서드입니다.
        /// 하위 클래스에서 오버라이드하여 구체적인 카드 효과를 구현합니다.
        /// </summary>
        /// <param name="owner">카드를 소유한 캐릭터</param>
        /// <param name="deck">카드가 속한 덱</param>
        /// <param name="eventType">발생한 이벤트 타입</param>
        /// <param name="param">이벤트와 함께 전달된 매개변수</param>
        public virtual void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
        }
    }
} 