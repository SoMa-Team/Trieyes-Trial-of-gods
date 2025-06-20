using UnityEngine;
using Actors;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("# Game Object")]
        public Player player;
        public PoolManager poolManager;

        [Header("# Game Control")]
        public float gameTime;
        public int killCount;

        public static GameManager instance;

        private void Awake()
        {
            instance = this;
            killCount = 0;
        }

        private void Update()
        {
            gameTime += Time.deltaTime;
        }
    }
}
