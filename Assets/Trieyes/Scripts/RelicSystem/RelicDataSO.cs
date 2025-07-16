#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace RelicSystem
{
    public class RelicDataSO: ScriptableObject
    {
        public int id;
        public string name; // 화염검
        public string description; // "검이 화염으로 변합니다."
        public List<int> attackComponentIDs; // "[0]"
        public List<int> filterAttackIDs; // "[1, 2]"
        public string? filterTag; // fire
    }
}