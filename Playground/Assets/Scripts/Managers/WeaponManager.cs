using UnityEngine;
using Stats;

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
        if (owner == null || owner.statSheet == null || !owner.isLive) return;

        timer += Time.deltaTime;
        float attackSpeed = owner.statSheet[StatType.AttackSpeed].Value;
        if (attackSpeed > 0 && timer > 1f / attackSpeed)
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

        // StatSheet에서 스탯값을 가져옴
        int projectileCount = owner.statSheet[StatType.ProjectileCount].Value;
        float projectileSpeed = owner.statSheet[StatType.AttackSpeed].Value; // 혹시 ProjectileSpeed StatType이 따로 있으면 그걸 사용!
        float attackRange = owner.statSheet[StatType.AttackRange].Value;

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.Init(fireDir, projectileCount, projectileSpeed, attackRange, owner);
    }
}