using UnityEngine;
using CharacterSystem;
namespace NodeStage
{
    public abstract class NodeStage
    {
        public abstract void Activate(Character mainCharacter);
        public abstract void DeActivate();
        public abstract void NextStage();
    }
}