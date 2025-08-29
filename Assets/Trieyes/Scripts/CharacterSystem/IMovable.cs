using UnityEngine;

namespace CharacterSystem
{
    /// <summary>
    /// 이동 가능한 객체를 나타내는 인터페이스
    /// </summary>
    public interface IMovable
    {
        // ===== [인터페이스 메서드] =====
        void Move(Vector2 direction);
    }
} 