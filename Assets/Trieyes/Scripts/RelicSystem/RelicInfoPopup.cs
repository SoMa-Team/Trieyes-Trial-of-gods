using System;
using Cysharp.Threading.Tasks;
using RelicSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicInfoPopup : MonoBehaviour
{
    public static RelicInfoPopup Instance { private set; get; }
    
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        rectTransform.anchoredPosition = Vector2.zero;
        gameObject.SetActive(false);
        
        Instance = this;
    }

    public async void Create(Relic relic)
    {
        titleText.text = relic.name;
        descriptionText.text = relic.description;
        rectTransform.anchoredPosition = Vector2.zero;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        
        await cancelButton.OnClickAsync();
        
        gameObject.SetActive(false);
    }
}
