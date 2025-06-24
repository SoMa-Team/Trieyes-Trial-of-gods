using UnityEngine;

namespace CharacterSystem
{
    public abstract class Controller : MonoBehaviour
    {
        public Pawn owner;

        public virtual void Initialize(Pawn pawn)
        {
            owner = pawn;
        }

        public abstract void ProcessInput();
    }
}