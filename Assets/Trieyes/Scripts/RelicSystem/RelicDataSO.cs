#nullable enable
using UnityEngine;

namespace RelicSystem
{
    public class RelicDataSO: ScriptableObject
    {
        public string description;
        public string attackComponentIDs;
        public int? filterAttackID;
        public string? filterTag;
    }
}