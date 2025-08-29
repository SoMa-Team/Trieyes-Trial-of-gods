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
    //TODO: CSV 파일들 하나로 합치기, ProjectileCount와 ProjectilePierce 없애기
    public string characterName;
    public List<StatValuePair> stats;
}