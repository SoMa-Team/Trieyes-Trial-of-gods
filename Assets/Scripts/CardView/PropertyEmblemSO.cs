using System.Collections;
using UnityEngine;
using CardSystem;
namespace CardView
{
    /// <summary>
    /// 카드 속성별 엠블럼(아이콘) 정보를 저장하는 ScriptableObject입니다.
    /// 속성에 따라 엠블럼 스프라이트를 반환합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Card/PropertyEmblemTable")]
    public class PropertyEmblemSO : ScriptableObject
    {
        /// 속성과 엠블럼 스프라이트의 쌍을 저장하는 구조체
        [System.Serializable]
        public struct PropertyEmblemPair
        {
            /// 카드 속성
            public Property property;
            /// 속성에 해당하는 엠블럼 스프라이트
            public Sprite emblemSprite;
        }

        /// 속성-엠블럼 쌍 배열
        public PropertyEmblemPair[] emblems;

        /// <summary>
        /// 주어진 속성에 해당하는 엠블럼 스프라이트를 반환합니다.
        /// 없으면 null 반환
        /// </summary>
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