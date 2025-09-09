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
        public Sprite icon = null;
        public string description;
        
        public RelicAction relicAction;
        // Relic의 이벤트 Handler
        public List<int> filterAttackIDs;
        // 유물이 적용되는 공격 대상
        public List<int> attackComponentIDs;
        // 유물이 적용되는 공격에 부착되는 AttackComponent
        
        // ===== [기능 3] 이벤트 처리 =====
        [CanBeNull] private Pawn owner; // 유물의 소유자 (Pawn)

        public void AttachTo(Pawn owner)
        {
            this.owner = owner;
        }

        public virtual bool OnEvent(Utils.EventType eventType, object param)
        {
            return relicAction.OnEvent(eventType, param);
        }
    }
} 