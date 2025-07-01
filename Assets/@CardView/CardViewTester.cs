using UnityEngine;
using UnityEngine.UI;
using CardSystem;
using CardViews;

/// <summary>
/// CardView UI 테스트를 위한 컴포넌트입니다.
/// 버튼 클릭 시 다양한 카드를 CardView에 표시합니다.
/// </summary>
public class CardViewTester : MonoBehaviour
{
    // --- 인스펙터 필드 ---

    /// 테스트할 CardView 컴포넌트
    public CardView cardView;
    /// 카드 생성 팩토리
    public CardFactory cardFactory;
    /// '준비운동' 카드 표시 버튼
    public Button showPreparingMarch;
    /// '웅크리기' 카드 표시 버튼
    public Button showCrouch;
    /// '그림자' 카드 표시 버튼
    public Button showShadow;

    // --- Unity 이벤트 메서드 ---

    /// <summary>
    /// 버튼 클릭 이벤트를 등록합니다.
    /// </summary>
    void Awake()
    {
        showPreparingMarch.onClick.AddListener(ShowPreparingMarch);
        showCrouch.onClick.AddListener(ShowCrouch);
        showShadow.onClick.AddListener(ShowShadow);
    }

    // --- 카드 표시 메서드 ---

    /// <summary>
    /// '준비운동' 카드를 생성하여 CardView에 표시합니다.
    /// </summary>
    void ShowPreparingMarch()
    {
        Card card = cardFactory.Create(1, 0);
        cardView.SetCard(card);
    }

    /// <summary>
    /// '웅크리기' 카드를 생성하여 CardView에 표시합니다.
    /// </summary>
    void ShowCrouch()
    {
        Card card = cardFactory.Create(1, 1);
        cardView.SetCard(card);
    }
    
    /// <summary>
    /// '그림자' 카드를 생성하여 CardView에 표시합니다.
    /// </summary>
    void ShowShadow()
    {
        Card card = cardFactory.Create(1, 2);
        cardView.SetCard(card);
    }
}