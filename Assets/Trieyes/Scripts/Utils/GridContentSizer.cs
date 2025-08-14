using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class GridContentSizer : MonoBehaviour
    {
        GridLayoutGroup g; RectTransform rt;

        void Awake(){ g = GetComponent<GridLayoutGroup>(); rt = (RectTransform)transform; }
        void OnEnable() => Refresh();
        void OnTransformChildrenChanged() => Refresh(); // 아이템 추가/제거 시 자동 반영

        public void Refresh()
        {
            int n = 0;
            for (int i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).gameObject.activeSelf) n++;

            float w = g.padding.left + g.padding.right
                                     + n * g.cellSize.x + Mathf.Max(0, n - 1) * g.spacing.x;
            float h = g.padding.top + g.padding.bottom
                                    + 1 * g.cellSize.y;

            rt.sizeDelta = new Vector2(w, h);
        }
    }
}