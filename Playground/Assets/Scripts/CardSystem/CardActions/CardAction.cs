using System.Collections.Generic;
using Utils; // For IEventHandler and EventType
using UnityEngine;
using Actors;

namespace CardActions
{
    /// <summary>
    /// 카드 액션의 기본 동작을 정의하는 추상 클래스입니다.
    /// 모든 구체적인 카드 액션은 이 클래스를 상속받아 고유한 OnEvent 로직을 구현해야 합니다.
    /// </summary>
    public abstract class CardAction : MonoBehaviour, IEventHandler
    {
        [Header("Action Info")]
        public int cardId;
        [System.NonSerialized] public string cardDescription;

        protected Pawn owner; // 카드 액션의 소유자 (Pawn)
        protected CardSystem.Deck deck; // 덱 참조 (카드 호출 순서 조정용)

        // ===== [기능 2] 소유자 설정 =====
        /// <summary>
        /// 카드 액션의 소유자를 설정합니다.
        /// </summary>
        /// <param name="pawn">카드 액션의 소유자</param>
        public void SetOwner(Pawn pawn)
        {
            owner = pawn;
            if (pawn != null)
            {
                deck = pawn.deck; // 덱 참조 설정
            }
        }

        // ===== [기능 3] 이벤트 처리(추상) =====
        /// <summary>
        /// 이벤트를 처리하는 추상 메서드입니다.
        /// 상속받는 클래스에서 특정 이벤트(예: OnAttack, OnDeath) 발생 시 수행할 고유한 로직을 구현합니다.
        /// </summary>
        /// <param name="eventType">발생한 이벤트의 타입</param>
        /// <param name="param">이벤트와 함께 전달된 매개변수</param>
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            // deck 참조가 없으면 자동으로 찾기
            EnsureDeckReference();
            
            // 하위 클래스에서 이 메서드를 오버라이드하여
            // 개별 이벤트에 대한 구체적인 로직을 구현합니다.
        }

        /// <summary>
        /// deck 참조가 설정되어 있는지 확인하고, 없으면 자동으로 찾아서 설정합니다.
        /// </summary>
        protected void EnsureDeckReference()
        {
            if (deck == null)
            {
                if (owner != null)
                {
                    deck = owner.deck;
                    Debug.Log($"<color=green>[CardAction] Found deck through owner: {owner.gameObject.name}</color>");
                }
                else
                {
                    // owner도 없으면 현재 씬에서 Pawn을 찾아서 deck 참조 설정
                    Pawn foundPawn = FindFirstObjectByType<Pawn>();
                    if (foundPawn != null)
                    {
                        owner = foundPawn;
                        deck = foundPawn.deck;
                        Debug.Log($"<color=green>[CardAction] Found deck through scene search: {foundPawn.gameObject.name}</color>");
                    }
                    else
                    {
                        Debug.LogError("<color=red>[CardAction] No Pawn found in scene!</color>");
                    }
                }
            }
        }
    }
} 