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
        protected List<ActionParam> actionParams;
        public virtual void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            // 기본 구현은 비어있습니다.
            // 하위 클래스에서 오버라이드하여 구체적인 로직을 구현합니다.
        }

        public virtual object GetEffectiveParam(int index, Card card)
        {
            // 카드에 스티커가 있으면 오버라이드
            if (card.stickerOverrides != null &&
                card.stickerOverrides.TryGetValue(index, out Sticker sticker))
            {
                // 각 액션별로, 각 파라미터 인덱스별 타입 처리
                if (sticker.type == StickerType.Number)
                    return sticker.numberValue;
                else if (sticker.type == StickerType.StatType)
                    return sticker.statTypeValue;
            }

            // 없으면 기본 파라미터 반환 (액션별 구현)
            return GetBaseParam(index, card);
        }

        public virtual CardAction DeepCopy()
        {
            return this;
            //카드 액션이 상태를 갖는 경우 DeepCopy 로직을 추가합니다.
        }

        public virtual string[] GetDescriptionParams(Card card)
        {
            var descParams = new string[ParamCount];
            for (int i = 0; i < ParamCount; i++)
            {
                var val = GetEffectiveParam(i, card);
                if (actionParams[i].kind == ParamKind.StatType)
                    descParams[i] = StatTypeTransformer.StatTypeToKorean((StatType)val);
                else
                    descParams[i] = val.ToString();
            }
            return descParams;
        }
        public abstract object GetBaseParam(int index, Card card);
        
        public int ParamCount => actionParams.Count;
        public ActionParam GetParamDef(int index) => actionParams[index];
    }
} 