using CharacterSystem;
using System.Collections.Generic;

namespace GameFramework
{
    public class ShopStageManager
    {
        // ===== [기능 1] 상점 캐릭터 관리 =====
        public List<Pawn> playerCharactersInStore = new List<Pawn>();
        public void InitializeStore(List<Pawn> characters)
        {
            playerCharactersInStore = characters;
            // 상점 UI 설정, 상품 로드 등의 초기화 로직
        }

        // ===== [기능 2] 상점 기능 =====
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