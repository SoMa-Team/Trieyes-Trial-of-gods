using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public int prefabId;
    private float timer;
    private Camera mainCamera;

    private Actor owner;

    private void Awake()
    {
        mainCamera = Camera.main;
        owner = GetComponentInParent<Actor>();
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
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector3 fireDir = mouseWorldPos - owner.transform.position;

        GameObject bulletObj = GameManager.instance.poolManager.Get(prefabId);
        bulletObj.transform.position = owner.transform.position;
        bulletObj.transform.up = fireDir.normalized;

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        var stats = owner.statManager.attakStats;
        bullet.Init(fireDir, stats.projectileCount, stats.projectileSpeed, stats.attackRange, owner);
    }
}
