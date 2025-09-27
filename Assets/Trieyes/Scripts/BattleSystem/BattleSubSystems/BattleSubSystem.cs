using UnityEngine;
using System.Collections.Generic;

namespace BattleSystem
{
    /// <summary>
    /// 비콘 오브젝트 - 캐릭터가 일정 시간 동안 머물면 BattleTimerStage에 콜백을 보내는 컴포넌트
    /// </summary>
    public class BattleSubsystem : MonoBehaviour
    {
        protected virtual void Awake()
        {
            BattleStage.now.subsystems.Add(this);
        }

        public virtual void Activate()
        {
            return;
        }

        public virtual void Deactivate()
        {
            BattleStage.now.subsystems.Remove(this);
            Destroy(gameObject);
            return;
        }
    }
}
