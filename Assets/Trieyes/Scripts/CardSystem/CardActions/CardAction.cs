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
        protected Card card;

        // 생성자 대신 Setter로도 가능
        public void SetCard(Card card)
        {
            this.card = card;
        }

        protected List<ActionParam> actionParams;

        public virtual void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            // 하위 클래스에서 구현
        }

        public virtual object GetEffectiveParam(int index)
        {
            // sticker override가 있으면 적용
            if (card.stickerOverrides != null &&
                card.stickerOverrides.TryGetValue(index, out Sticker sticker))
            {
                if (sticker.type == StickerType.Number)
                    return sticker.numberValue;
                else if (sticker.type == StickerType.StatType)
                    return sticker.statTypeValue;
            }
            // 없으면 기본값
            return GetBaseParam(index);
        }

        public virtual CardAction DeepCopy()
        {
            // 상태가 있을 경우 복제 로직
            return this;
        }

        public virtual string[] GetDescriptionParams()
        {
            var descParams = new string[ParamCount];
            for (int i = 0; i < ParamCount; i++)
            {
                var val = GetEffectiveParam(i);
                if (actionParams[i].kind == ParamKind.StatType)
                    descParams[i] = StatTypeTransformer.StatTypeToKorean((StatType)val);
                else
                    descParams[i] = val.ToString();
            }
            return descParams;
        }

        // GetBaseParam에서 card 직접 사용
        public virtual object GetBaseParam(int index)
        {
            return actionParams[index].getBaseValue(card);
        }

        public int ParamCount => actionParams.Count;
        public ActionParam GetParamDef(int index) => actionParams[index];
    }
} 