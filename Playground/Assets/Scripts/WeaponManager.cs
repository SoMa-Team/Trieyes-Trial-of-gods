using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public int prefabId;
    private float timer;
    private Camera mainCamera;

    private Player owner;

    private void Awake()
    {
        mainCamera = Camera.main;
        owner = GetComponentInParent<Player>();
    }

    private void Update()
    {
        if (owner == null || owner.statManager == null || !owner.isLive) return;

        timer += Time.deltaTime;
        if (timer > 1f / owner.statManager.attakStats.attackSpeed)
        {
            timer = 0f;
            Fire();
        }
    }
    void Fire()
    {
        Vector2 inputVec = owner.inputVec;
        
        if (inputVec == Vector2.zero)
            return;

        Vector3 fireDir = new Vector3(inputVec.x, inputVec.y, 0f);

        GameObject bulletObj = GameManager.instance.poolManager.Get(prefabId);
        bulletObj.transform.position = owner.transform.position;
        bulletObj.transform.up = fireDir.normalized;

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        var stats = owner.statManager.attakStats;
        bullet.Init(fireDir, stats.projectileCount, stats.projectileSpeed, stats.attackRange, owner);
    }
}
