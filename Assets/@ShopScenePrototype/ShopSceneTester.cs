using UnityEngine;
using CardViews;
using UnityEngine.UI;
using CardSystem;
using DeckViews;

public class ShopSceneTester : MonoBehaviour
{
    public CardView cardView1;
    public CardView cardView2;
    public CardView cardView3;

    public Button BuyButton1;
    public Button BuyButton2;
    public Button BuyButton3;

    public Button RerollButton;
    
    public CardFactory cardFactory;

    private Deck playerDeck;

    private void Start()
    {
        Reroll();
        
        playerDeck = new Deck();

        RerollButton.onClick.AddListener(Reroll);
        BuyButton1.onClick.AddListener(BuyCard1);
        BuyButton2.onClick.AddListener(BuyCard2);
        BuyButton3.onClick.AddListener(BuyCard3);
    }

    private void Reroll()
    {
        Card card1 = cardFactory.Create(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(0, cardFactory.cardInfos.Count));
        Card card2 = cardFactory.Create(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(0, cardFactory.cardInfos.Count));
        Card card3 = cardFactory.Create(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(0, cardFactory.cardInfos.Count));
        
        cardView1.SetCard(card1);
        cardView2.SetCard(card2);
        cardView3.SetCard(card3);
    }

    public void BuyCard1()
    {
        var card = cardView1.GetCurrentCard();
        playerDeck.AddCard(card);
        DeckZoneManager.Instance.RefreshDeckUI(playerDeck);
    }

    public void BuyCard2()
    {
        var card = cardView2.GetCurrentCard();
        playerDeck.AddCard(card);
        DeckZoneManager.Instance.RefreshDeckUI(playerDeck);
    }

    public void BuyCard3()
    {
        var card = cardView3.GetCurrentCard();
        playerDeck.AddCard(card);
        DeckZoneManager.Instance.RefreshDeckUI(playerDeck);
    }
}
