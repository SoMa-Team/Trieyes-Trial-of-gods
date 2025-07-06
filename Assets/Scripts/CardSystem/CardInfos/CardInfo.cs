using UnityEngine;
using System.Collections.Generic;

namespace CardSystem
{
/// <summary>
/// 카드의 기본 정보를 담는 클래스입니다.
/// 카드의 속성, 희귀도, 이름, 이미지, 설명 등 UI 표시에 필요한 정보를 관리합니다.
/// CardAction과 분리되어 데이터와 로직을 명확히 구분합니다.
/// </summary>
public class CardInfo : ScriptableObject{
    /// <summary>
    /// 이 카드가 가진 속성들의 배열입니다.
    /// 카드의 기본 스탯을 결정하는 데 사용됩니다.
    /// </summary>
    public Property[] properties;
    /// <summary>
    /// 카드의 희귀도입니다.
    /// 카드의 등급과 강화 가능성을 결정합니다.
    /// </summary>
    public Rarity rarity;
    /// <summary>
    /// 카드의 이름입니다.
    /// UI에서 표시되는 카드의 제목입니다.
    /// </summary>
    public string cardName;
    /// <summary>
    /// 카드의 일러스트레이션 이미지입니다.
    /// UI에서 카드를 시각적으로 표현하는 데 사용됩니다.
    /// </summary>
    public Sprite illustration;
    /// <summary>
    /// 카드의 설명 텍스트입니다.
    /// UI에서 카드의 효과를 설명하는 데 사용됩니다.
    /// </summary>
    [TextArea] public string cardDescription;
    /// <summary>
    /// 해당 카드가 반응하는 이벤트 타입들의 리스트입니다.
    /// 이벤트 처리 최적화를 위해 사용됩니다.
    /// </summary>
    public List<Utils.EventType> eventTypes = new();
    
    [HideInInspector]
    public string[] descParams;
}
}