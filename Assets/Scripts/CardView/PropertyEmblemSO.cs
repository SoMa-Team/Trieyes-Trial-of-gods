using System.Collections;
using UnityEngine;
using CardSystem;
namespace CardView
{
    
    [CreateAssetMenu(menuName = "Card/PropertyEmblemTable")]
    public class PropertyEmblemSO : ScriptableObject
    {
        [System.Serializable]
        public struct PropertyEmblemPair
        {
            public Property property;
            public Sprite emblemSprite;
        }

        public PropertyEmblemPair[] emblems;

        public Sprite GetEmblem(Property property)
        {
            Debug.Log($"GetEmblem 호출: property = {property}, 타입 = {property.GetType()}");
            foreach(var pair in emblems)
                if(pair.property == property)
                    return pair.emblemSprite;
            return null;
        }

    }
}