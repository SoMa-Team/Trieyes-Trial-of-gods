using System.Collections.Generic;
using Utils;
using UnityEngine;
using CharacterSystem;

namespace RelicSystem
{
    /// <summary>
    /// 게임 내 유물을 나타내는 클래스입니다.
    /// 유물은 자체적으로 이벤트를 등록하고 처리할 수 있는 IEventHandler를 구현합니다.
    /// </summary>
    public abstract class Relic : IEventHandler
    {
        // ===== [기능 1] 유물 정보 및 생성 =====
        public RelicInfo info;
        protected Pawn owner; // 유물의 소유자 (Pawn)
        
        public Relic(RelicInfo info)
        {
            this.info = info;
        }

        // ===== [기능 2] 소유자 설정 =====
        /// <summary>
        /// 유물의 소유자를 설정합니다.
        /// </summary>
        /// <param name="pawn">유물의 소유자</param>
        public void SetOwner(Pawn pawn)
        {
            owner = pawn;
        }

        // ===== [기능 3] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            // owner 참조가 없으면 자동으로 찾기
            EnsureOwnerReference();

            // 하위 클래스에서 이 메서드를 오버라이드하여
            // 개별 이벤트에 대한 구체적인 로직을 구현합니다.
        }

        /// <summary>
        /// owner 참조가 설정되어 있는지 확인하고, 없으면 자동으로 찾아서 설정합니다.
        /// </summary>
        protected void EnsureOwnerReference()
        {
            if (owner == null)
            {
                // 현재 씬에서 Pawn을 찾아서 owner 참조 설정
                Pawn foundPawn = UnityEngine.Object.FindFirstObjectByType<Pawn>();
                if (foundPawn != null)
                {
                    owner = foundPawn;
                    Debug.Log($"<color=green>[Relic] Found owner through scene search: {foundPawn.gameObject.name}</color>");
                }
                else
                {
                    Debug.LogError("<color=red>[Relic] No Pawn found in scene!</color>");
                }
            }
        }

        // ===== [기능 5] AttackComponent 제공 =====
        /// <summary>
        /// 이 유물이 제공하는 AttackComponent 목록을 반환합니다.
        /// 기본적으로는 null을 반환하며, 하위 클래스에서 오버라이드하여 구현합니다.
        /// </summary>
        /// <returns>AttackComponent 목록 또는 null</returns>
        public virtual List<AttackComponents.AttackComponent> GetAttackComponents()
        {
            return null; // 기본적으로는 AttackComponent를 제공하지 않음
        }
    }
} 