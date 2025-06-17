using CharacterSystem;
using System.Collections.Generic;

namespace GameFramework
{
    public class ShopStageManager
    {
        // 상점 레벨에서 관리할 필드 (예시)
        public List<Pawn> playerCharactersInStore = new List<Pawn>();

        // 상점 레벨 초기화 메서드
        public void InitializeStore(List<Pawn> characters)
        {
            playerCharactersInStore = characters;
            // 상점 UI 설정, 상품 로드 등의 초기화 로직
        }

        // 상점 특정 기능 (예: 아이템 구매, 판매) 메서드
        public void PurchaseItem()
        {
            // 아이템 구매 로직
        }

        public void SellItem()
        {
            // 아이템 판매 로직
        }
    }
} 