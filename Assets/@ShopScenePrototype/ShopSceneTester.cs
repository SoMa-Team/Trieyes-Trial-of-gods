using UnityEngine;
using CardViews;
using UnityEngine.UI;
using CardSystem;

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

    private void Awake()
    {
        Reroll();
        
        RerollButton.onClick.AddListener(Reroll);
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
}
