using UnityEngine;
using CharacterSystem;
namespace NodeStage
{
    public interface NodeStage
    {
        abstract void Activate(Character mainCharacter);
        abstract void NextStage();
    }
}