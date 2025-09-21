using System;
using UnityEngine;

namespace CharacterSystem
{
    /// <summary>
    /// 모든 컨트롤러의 기본 클래스입니다.
    /// </summary>
    public abstract class Controller : MonoBehaviour
    {
        // ===== [필드] =====
        public Pawn owner;

        protected bool lockMovement;
        
        public enum EnemyType
        {
            Follow,
            RangeAttackRun,
            RangeAttackOnly,
            Block,
            Boss
        }

        /// <summary>
        /// 현재 이동 방향
        /// </summary>
        private Vector2 _moveDir;
        public Vector2 moveDir
        {
            set
            {
                _moveDir = value;
                if (_moveDir.magnitude > 1e-6) 
                    lastMoveDir = _moveDir;
            }
            get => _moveDir;
        }
        public Vector2 lastMoveDir;
        public bool isAutoAttack;

        // ===== [Unity 생명주기] =====
        public virtual void Activate(Pawn pawn)
        {
            enabled = true;
            isAutoAttack = true;
            lockMovement = false;
            owner = pawn;
        }

        public virtual void ProcessInputActions()
        {
            
        }

        public virtual void Update()
        {
            owner.CalculateBasicAttackCooldown();
        }

        public virtual void Deactivate()
        {
            enabled = false;
            gameObject.SetActive(false);
        }

        public void SetLockMovement(bool lockMovement)
        {
            this.lockMovement = lockMovement;
        }
    }
}