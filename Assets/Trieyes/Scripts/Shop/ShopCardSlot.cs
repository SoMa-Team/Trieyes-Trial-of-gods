using CardSystem;
using UnityEngine;
using CardViews;
using TMPro;
using UnityEngine.Serialization;
using CharacterSystem;
using DeckViews;

public class ShopCardSlot : MonoBehaviour
{
    public CardView cardView;
    public TMP_Text priceText;
    public GameObject disableOverlay;

    private readonly int COMMON_PRICE = 40;
    private readonly int UNCOMMON_PRICE = 60;
    private readonly int LEGENDARY_PRICE = 70;
    private readonly int EXCEED_PRICE = 80;

    private int price;

    public void SetCard(Card card)
    {
        disableOverlay.SetActive(false);
        cardView.SetCard(card);
        switch (card.rarity)
        {
            case Rarity.Common:
                priceText.text = COMMON_PRICE.ToString();
                price = COMMON_PRICE;
                break;
            case Rarity.Uncommon:
                priceText.text = UNCOMMON_PRICE.ToString();
                price = UNCOMMON_PRICE;
                break;
            case Rarity.Legendary:
                priceText.text = LEGENDARY_PRICE.ToString();
                price = LEGENDARY_PRICE;
                break;
            case Rarity.Exceed:
                priceText.text = EXCEED_PRICE.ToString();
                price = EXCEED_PRICE;
                break;
            default:
                break;
        }
    }

    public Card GetCurrentCard()
    {
        return cardView.GetCurrentCard();
    }

    public void OnClickBuyButton()
    {
        Pawn mainCharacter=ShopSceneManager.Instance.mainCharacter;
        DeckView deckView=ShopSceneManager.Instance.deckView;
        if (mainCharacter.gold < price)
        {
            Debug.LogError("Not enough gold");
            return;
        }
        mainCharacter.gold -= price;
        mainCharacter.deck.AddCard(GetCurrentCard().DeepCopy());
        deckView.RefreshDeckUI();
        disableOverlay.SetActive(true);
    }
}
