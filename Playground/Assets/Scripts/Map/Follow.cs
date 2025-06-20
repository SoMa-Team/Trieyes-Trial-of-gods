using UnityEngine;

namespace Map
{
    public class Follow : MonoBehaviour
    {
        RectTransform rect;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rect = GetComponent<RectTransform>();
        }

        private void FixedUpdate()
        {
            rect.position = Camera.main.WorldToScreenPoint(GameManager.instance.player.transform.position);
        }
    }
}
