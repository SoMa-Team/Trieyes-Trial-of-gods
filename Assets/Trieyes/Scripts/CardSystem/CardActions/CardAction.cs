using System.Collections.Generic;
using Utils; // For IEventHandler and EventType
using UnityEngine;
using CharacterSystem;
using CardSystem;
using StickerSystem;
using Stats;

namespace CardActions
{
    /// <summary>
    /// 모든 카드 효과의 공통 인터페이스/기초 동작을 제공하는 추상 클래스.
    /// 각 카드 고유 효과는 이 클래스를 상속받아 구현한다.
    /// </summary>
    public abstract class CardAction
    {
        /// <summary>
        /// 이 액션이 적용될 카드 참조. (카드와 액션이 1:1 연결)
        /// </summary>
        protected Card card;

        /// <summary>
        /// 카드 객체를 할당한다. (필수: 카드 초기화 시 연결)
        /// </summary>
        public void SetCard(Card card)
        {
            this.card = card;
        }

        /// <summary>
        /// 액션에서 참조하는 파라미터 정의(타입, 이름 등).
        /// </summary>
        protected List<ActionParam> actionParams;

        /// <summary>
        /// 실제 카드 이벤트 동작 (하위 클래스에서 오버라이드)
        /// </summary>
        public virtual bool OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            return false;
            // 개별 카드별 효과 구현!
        }

        /// <summary>
        /// (스티커 등) 오버라이드된 파라미터 값을 반환.
        /// </summary>
        public virtual object GetEffectiveParam(int index)
        {
            // 1. 스티커 덮어쓰기 적용
            if (card.stickerOverrides != null &&
                card.stickerOverrides.TryGetValue(index, out Sticker sticker))
            {
                if (sticker.type == StickerType.Number)
                    return sticker.numberValue;
                else if (sticker.type == StickerType.StatType)
                    return sticker.statTypeValue;
                else if (sticker.type == StickerType.Probability)
                    return sticker.numberValue;
            }
            // 2. 오버라이드 없으면 원래 값 반환
            return GetBaseParam(index);
        }

        /// <summary>
        /// CardAction의 깊은 복사본을 반환 (필요 시 오버라이드)
        /// </summary>
        public virtual CardAction DeepCopy()
        {
            // 특별한 상태가 있으면 override해서 복제 구현!
            return this;
        }

        /// <summary>
        /// 파라미터의 기본값 반환 (card 데이터 기반)
        /// </summary>
        public virtual object GetBaseParam(int index)
        {
            return actionParams[index].getBaseValue(card);
        }

        /// <summary>
        /// 파라미터 개수
        /// </summary>
        public int ParamCount => actionParams?.Count ?? 0;

        /// <summary>
        /// 파라미터 정의 반환
        /// </summary>
        public ActionParam GetParamDef(int index) => actionParams[index];
    }
}
