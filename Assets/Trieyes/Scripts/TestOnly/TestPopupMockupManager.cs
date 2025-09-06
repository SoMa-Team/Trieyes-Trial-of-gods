using System;
using System.Collections.Generic;
using UnityEngine;

public class TestPopupMockupManager : MonoBehaviour
{
    [SerializeField] private List<RectTransform> transforms;

    private int index = 0;
    private void Start()
    {
        foreach (var rectTransform in transforms)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.gameObject.SetActive(false);
        }
        
        transforms[0].gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var next = (index + 1) % transforms.Count;
            Debug.Log($"{index} -> {next}");
            if (index != 0)
                transforms[index].gameObject.SetActive(false);
            transforms[next].gameObject.SetActive(true);
            index = next;
        }
    }
}
