using System.Collections.Generic;
using Utils;
using AttackSystem;

namespace AttackComponents
{
    /// <summary>
    /// 공격 관련 컴포넌트의 기본 동작을 정의하는 추상 클래스입니다.
    /// 이 컴포넌트는 IEventHandler를 구현하여 이벤트를 처리할 수 있습니다.
    /// </summary>
    public abstract class AttackComponent : IEventHandler
    {
        // ===== [기능 1] 이벤트 처리(추상) =====
        public abstract void OnEvent(Utils.EventType eventType, object param);

        // ===== [기능 2] 공격 컴포넌트 실행 및 이벤트 반응 =====
        public abstract void Execute(Attack attack);
    }
} 