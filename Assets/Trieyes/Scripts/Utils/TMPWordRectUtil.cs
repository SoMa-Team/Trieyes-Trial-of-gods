using UnityEngine;
using TMPro;
namespace Utils
{
    public static class TMPWordRectUtil
    {
        public static Rect GetLocalRect(this TMP_WordInfo wordInfo, TMP_Text text)
        {
            if (text == null || wordInfo.firstCharacterIndex < 0 || wordInfo.lastCharacterIndex < 0) return Rect.zero;

            var charInfo0 = text.textInfo.characterInfo[wordInfo.firstCharacterIndex];
            var charInfo1 = text.textInfo.characterInfo[wordInfo.lastCharacterIndex];

            // TMP에서 local 좌표는 pivot(0,1) 기준 (좌상단)임에 유의
            float xMin = charInfo0.bottomLeft.x;
            float xMax = charInfo1.topRight.x;
            float yMax = charInfo0.topRight.y;
            float yMin = charInfo0.descender; // 보통은 topRight.y - lineHeight 도 쓸 수 있음

            float width = xMax - xMin;
            float height = yMax - yMin;

            return new Rect(xMin, yMax, width, height); // 좌상단 기준 Rect
        }
    }
}