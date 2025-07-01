using System.Collections;
using UnityEngine;
using Stats;

namespace CardView
{
    [CreateAssetMenu(menuName = "Card/StatTypeEmblemTable")]
    public class StatTypeEmblemSO : ScriptableObject
    {
        [System.Serializable]
        public struct StatTypeEmblemPair
        {
            public StatType statType;
            public Sprite emblemSprite;
        }

        public StatTypeEmblemPair[] emblems;

        public Sprite GetEmblem(StatType statType)
        {
            foreach(var pair in emblems)
                if(pair.statType == statType)
                    return pair.emblemSprite;
            return null;
        }
    }
}