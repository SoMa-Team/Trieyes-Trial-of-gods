using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // 따라갈 대상(캐릭터)
    public Vector3 offset = new Vector3(0, 0, -10); // 카메라와의 거리(기본값: z축 -10)

    void LateUpdate()
    {
        if (target == null) return;
        transform.position = target.position + offset;
    }
}