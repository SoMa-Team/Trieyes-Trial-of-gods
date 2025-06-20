using Stats;
using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public struct StatValuePair
{
    public StatType type;
    public int value;
}
[CreateAssetMenu(menuName = "Stats/StatPreset")]
public class StatPresetSO : ScriptableObject
{
    public List<StatValuePair> stats;
}
