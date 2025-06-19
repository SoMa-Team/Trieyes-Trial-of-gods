using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Spawn Settings")]
    public int enemyPrefabIndex;
    public float baseInterval;

    private float timer=0;

    private void Awake()
    {
        spawnPoints = GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        if (GameManager.instance.player == null || GameManager.instance.player.statManager == null)
            return;

        float multiplier = GameManager.instance.player.statManager.utilityStats.spawnQuantityMultiplier;

        timer += Time.deltaTime;
        if (timer > baseInterval / multiplier)
        {
            timer = 0f;
            Spawn();
        }
    }
    private void Spawn()
    {
        GameObject enemy = GameManager.instance.poolManager.Get(enemyPrefabIndex);
        enemy.transform.position = spawnPoints[Random.Range(1, spawnPoints.Length)].position;
    }
}
