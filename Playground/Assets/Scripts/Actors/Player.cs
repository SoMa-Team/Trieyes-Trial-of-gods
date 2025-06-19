using UnityEngine;

public class Player : Actor
{
    public Vector2 inputVec;

    private Vector3 fixedDeathPosition;

    void Update()
    {
        if (!isLive) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mousePos - transform.position;
        inputVec = dir.normalized;
    }

    void FixedUpdate()
    {
        if (!isLive) return;
        Vector2 nextVec = inputVec * statManager.utilityStats.moveSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    private void LateUpdate()
    {
        if (!isLive)
        {
            transform.position = fixedDeathPosition;
            return;
        }
        animator.SetFloat("Speed", inputVec.magnitude); 
        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }

    protected override void Die()
    {
        fixedDeathPosition = transform.position;
        base.Die();
    }
}
