using CardSystem;
using CardViews;
using StickerSystem;
using UnityEngine;
using Utils;

public class Initer : MonoBehaviour
{
    public CardView cardView;

    public void OnClick()
    {
        Card card = CardFactory.Instance.RandomCreate();
        cardView.SetCard(card);
    }

    private void Start()
    {
        Sticker sticker = new Sticker();
        sticker.type = StickerType.Number;
        sticker.numberValue = 67;
        ShopSceneManager.Instance.selectedSticker = sticker;
    }
}
