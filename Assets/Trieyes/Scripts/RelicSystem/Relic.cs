using System;
using System.Collections.Generic;
using Utils;
using CharacterSystem;
using JetBrains.Annotations;
using TagSystem;
using UnityEngine;

namespace RelicSystem
{
    /// <summary>
    /// 게임 내 유물을 나타내는 클래스입니다.
    /// 유물은 자체적으로 이벤트를 등록하고 처리할 수 있는 IEventHandler를 구현합니다.
    /// </summary>
    public class Relic : IEventHandler
    {
        // ===== [기능 1] 유물 정보 및 생성 =====
        public int relicID;
        public string name;
        public string description;
        public List<int> filterAttackIDs;
        public AttackTag? filterAttackTag;
        public List<int> attackComponentIDs;

        public List<RandomOption> randomOptions;
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        


        // ===== [기능 3] 이벤트 처리 =====
        [CanBeNull] public Pawn owner; // 유물의 소유자 (Pawn)
        public List<Utils.EventType> acceptedEvents = new List<Utils.EventType>();
        
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
        // owner 참조가 없으면 에러 발생
        if (owner == null)
        {
            Debug.LogError($"<color=red>[Relic] {name ?? "Unknown"} has no owner reference! Ensure SetOwner() is called before using this relic.</color>");
            return;
        }
        
        Debug.Log($"<color=purple>[Relic] {name ?? "Unknown"} received event: {eventType} (accepted events: {string.Join(", ", acceptedEvents)})</color>");
        
        // 하위 클래스에서 이 메서드를 오버라이드하여
        // 개별 이벤트에 대한 구체적인 로직을 구현합니다.
        }

        public List<Utils.EventType> GetEventType()
        {
            return acceptedEvents;
        }

        // ===== [기능 6] 이벤트 필터링 =====

        /// <summary>
        /// 이 유물이 받을 이벤트 목록을 반환합니다.
        /// 기본적으로는 빈 HashSet을 반환하며, 하위 클래스에서 오버라이드하여 구현합니다.
        /// </summary>
        /// <returns>받을 이벤트들의 HashSet</returns>
        public virtual HashSet<Utils.EventType> GetAcceptedEvents()
        {
            if (acceptedEvents != null)
            {
                return new HashSet<Utils.EventType>(acceptedEvents);
            }
            return new HashSet<Utils.EventType>(); // 기본적으로는 모든 이벤트를 받지 않음
        }
    }
} 