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

        // ===== [Unity 생명주기] =====
        public virtual void Initialize(Pawn pawn)
        {
            owner = pawn;
        }

        public abstract void ProcessInputActions();
    }
}