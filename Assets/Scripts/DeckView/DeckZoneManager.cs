using UnityEngine;
using System.Collections.Generic;
using CardSystem;

public class DeckZoneManager : MonoBehaviour
{
    public Transform deckZone;
    public GameObject cardPrefab;

    private List<GameObject> cardViews = new();
    // 덱 UI 갱신
    public void RefreshDeckUI(Deck deck)
    {
        // 기존 카드 오브젝트 제거
        foreach (var obj in cardViews)
            Destroy(obj);
        cardViews.Clear();

        // 덱에 있는 카드 수 만큼 프리팹 인스턴스화
        foreach (var card in deck.Cards)
        {
            var cardViewInstance = Instantiate(cardPrefab, deckZone);
            // 카드 정보 반영 (CardView 스크립트의 SetCard 활용)
            cardViewInstance.GetComponent<CardViews.CardView>().SetCard(card);
            cardViews.Add(cardViewInstance);
        }
    }

}
