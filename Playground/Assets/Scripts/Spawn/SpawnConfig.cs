using UnityEngine;

namespace Spawn
{
    public class SpawnConfig : MonoBehaviour
    {
        public float baseSpawnInterval;

        public static SpawnConfig instance;
        private void Awake()
        {
            instance = this;
        }
    }
}
