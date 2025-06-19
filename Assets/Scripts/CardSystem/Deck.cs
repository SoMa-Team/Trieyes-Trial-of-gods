using System.Collections.Generic;
using Utils;
using CharacterSystem;
using Stats;
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 카드 덱을 관리하는 클래스입니다.
    /// 덱은 자체적으로 이벤트를 등록하고 처리할 수 있는 IEventHandler를 구현합니다.
    /// </summary>
    public class Deck : IEventHandler
    {
        // ===== [기능 1] 덱 기본 정보 =====
        public bool isPersistent { get; private set; }  // 덱이 영구적인지 여부 (메인 캐릭터의 덱은 true, 적의 덱은 false)
        public Pawn owner { get; private set; }        // 덱의 소유자

        // ===== [기능 2] 덱 상태 관리 =====
        public List<Card> cards = new();               // 현재 덱의 카드들

        // ===== [기능 3] 덱 초기화/정리 =====
        public void Initialize(Pawn owner, bool isPersistent)
        {
            this.owner = owner;
            this.isPersistent = isPersistent;
            Clear();
            
            // 모든 카드의 CardAction에 owner 설정
            foreach (var card in cards)
            {
                card?.SetOwner(owner);
            }
        }

        public void Clear()
        {
            if (!isPersistent)
            {
                cards.Clear();

            }
        }

        // ===== [기능 4] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            switch (eventType)
            {
                case Utils.EventType.OnBattleStart:
                    HandleDeckWhenBattleStart();
                    break;
                case Utils.EventType.OnBattleEnd:
                    HandleDeckWhenBattleEnd();
                    break;
                case Utils.EventType.OnCardPurchase:
                    HandleCardPurchase(param);
                    break;
                default:
                    break;
            }
        }

        private void HandleDeckWhenBattleStart()
        {
            // 1. 덱의 기본 스탯 계산
            StatSheet deckStats = CalcBaseStat();
            
            // 2. Pawn에게 덱 스탯 더하고 전달
            if (owner != null)
            {
                // 모든 스탯을 덱 스탯으로 덮어쓰기
                foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
                {
                    int deckStatValue = deckStats[statType].Value;
                    owner.statSheet[statType].SetBasicValue(deckStatValue);
                }
            }
            
            // 3. 카드 액션 초기화
            CalcActionInitStat(Utils.EventType.OnBattleSceneChange);
        }

        private void HandleDeckWhenBattleEnd()
        {
            if (!isPersistent)
            {
                // 적의 덱은 전투 종료 시 정리
                Clear();
                // 스탯 초기화
                if (owner != null)
                {
                    foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
                    {
                        owner.statSheet[statType].SetBasicValue(0);
                    }
                }
            }
            else
            {
                // 메인 캐릭터의 덱은 유지
            }
        }

        private void HandleCardPurchase(object param)
        {
            if (param is Card purchasedCard && isPersistent)
            {
                // 상점에서 카드 구매 시 덱에 추가
                cards.Add(purchasedCard);
            }
        }

        // ===== [기능 1] 카드 리스트 및 생성 =====
        public void AddCard(Card card)
        {
            if (card != null)
            {
                cards.Add(card);
                card.SetOwner(owner);
            }
        }

        public void RemoveCard(Card card)
        {
            if (card != null)
            {
                cards.Remove(card);
            }
        }

        // ===== [기능 2] 덱 스탯 및 카드 액션 초기화 =====
        /// <summary>
        /// 덱의 모든 카드 스탯을 합산하여 반환합니다.
        /// 이 합산된 스탯은 Pawn의 기본 스탯(statinfos)에 적용됩니다.
        /// </summary>
        /// <returns>합산된 StatInfo 리스트</returns>
        public StatSheet CalcBaseStat()
        {
            StatSheet emptyStat = new();
            return emptyStat;
        }

        /// <summary>
        /// 덱의 모든 카드를 순회하며 특정 이벤트 타입에 해당하는 CardAction의 OnEvent를 발동합니다.
        /// 이 메서드는 Pawn으로부터 특정 이벤트 (예: 사망)가 발생했음을 전달받아 덱 내의 카드 액션들이 반응하도록 합니다.
        /// </summary>
        /// <param name="eventType">발동할 이벤트 타입 (예: Utils.EventType.OnBattleStart)</param>
        /// <param name="param">이벤트 매개변수</param>
        public void CalcActionInitStat(Utils.EventType eventType, object param = null)
        {
            foreach (var card in cards)
            {
                if (card != null && card.cardAction != null)
                {
                    card.TriggerCardEvent(eventType, param);
                }
            }
        }
    }
} 