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
    /// </summary>
    public abstract class CardAction : ScriptableObject
    {
        [Header("Action Info")]
        public Property[] properties;
        public Rarity rarity;
        public string cardName;
        public Sprite illustration;
        [TextArea] public string cardDescription;
        public List<Utils.EventType> eventTypes = new();//해당 카드 액션이 반응하는 이벤트 타입
        
        public virtual void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
        }
    }
} 