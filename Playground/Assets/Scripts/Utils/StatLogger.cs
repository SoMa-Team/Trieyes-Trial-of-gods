using UnityEngine;
using System.Collections;
using Actors;

namespace Utils
{
    public class StatLogger : MonoBehaviour
    {
        public Player player;  // Inspector에서 할당 or 자동 찾기
        public float interval = 1f;

        private void Awake()
        {
            if (player == null)
                player = FindObjectOfType<Player>();
        }

        private void Start()
        {
            StartCoroutine(LogPlayerAttackPowerRoutine());
        }

        private IEnumerator LogPlayerAttackPowerRoutine()
        {
            while (true)
            {
                if (player != null)
                    Debug.Log("플레이어 현재 공격력: " + player.statSheet[Stats.StatType.AttackPower].Value);
                yield return new WaitForSeconds(interval);
            }
        }
    }
}