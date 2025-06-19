using System.Collections.Generic;
using Utils;
using UnityEngine;

namespace RelicSystem
{
    /// <summary>
    /// 게임 내 유물을 나타내는 클래스입니다.
    /// 유물은 자체적으로 이벤트를 등록하고 처리할 수 있는 IEventHandler를 구현합니다.
    /// </summary>
    public abstract class Relic : IEventHandler
    {
        // ===== [기능 1] 유물 정보 및 생성 =====
        public RelicInfo info;
        public Relic(RelicInfo info)
        {
            this.info = info;
        }

        // ===== [기능 2] 이벤트 처리 =====
        public abstract void OnEvent(Utils.EventType eventType, object param);
    }
} 