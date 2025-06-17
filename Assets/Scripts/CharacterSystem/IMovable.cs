using UnityEngine;

namespace CharacterSystem
{
    public interface IMovable
    {
        // ===== [기능 1] 이동 =====
        void Move(Vector2 direction);
    }
} 