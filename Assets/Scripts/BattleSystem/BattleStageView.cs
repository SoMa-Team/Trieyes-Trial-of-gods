using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BattleSystem
{
    /// <summary>
    /// 전투 스테이지의 뷰 컴포넌트
    /// BattleStage 데이터와 Unity GameObject를 연결하는 역할을 합니다.
    /// </summary>
    public class BattleStageView : MonoBehaviour
    {
        // ===== 뷰 데이터 =====
        private BattleStage _battleStage;
        public GameObject spriteRectPrefabs;
        public TileBase ruleTile; // 인스펙터에서 사용할 타일 배열 할당
        public int Height { get; internal set; }
        public int Width { get; internal set; }
        
        /// <summary>
        /// BattleStage 데이터에 대한 접근자
        /// 설정 시 양방향 연결을 자동으로 구성합니다.
        /// </summary>
        public BattleStage BattleStage
        {
            set
            {
                Height = 30;
                Width = 30;
                _battleStage = value;
                _battleStage.View = this;
                
                // TODO: 데이터 UI 동기화 로직 구현 필요
                CreateSpriteRect();
            }

            get
            {
                return _battleStage;
            }
        }

        private void CreateSpriteRect()
        {
            // TODO : 테스트 이후 제거 필요.
            if (spriteRectPrefabs is null || ruleTile is null) return;
            // TODO END
            
            // 1. 프리팹을 씬에 인스턴스화
            GameObject tilemapGO = Instantiate(spriteRectPrefabs);

            // 2. Tilemap 컴포넌트 가져오기
            Tilemap tilemap = tilemapGO.GetComponentInChildren<Tilemap>();
            if (tilemap == null)
            {
                Debug.LogError("Tilemap 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = -1;
            }

            // 3. 원하는 위치에 타일 그리기 (예시: 전체를 같은 타일로 채우기)
            // -에서 시작해서 +방향으로 그리기
            for (int y = Height / 2; y >= -Height / 2; y--)
            {
                for (int x = -Width / 2; x < Width / 2; x++)
                {
                    // 예시: tiles[0]을 전체에 채움. 필요시 타입별로 분기
                    tilemap.SetTile(new Vector3Int(x, y, 0), ruleTile);

                    // Z-order -1
                    tilemap.SetTileFlags(new Vector3Int(x, y, 0), TileFlags.None);
                }
            }
        }
    }
} 