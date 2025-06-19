using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    Collider2D coll;

    private void Awake()
    {
        coll = GetComponent<Collider2D>();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area")) return;

        Vector3 PlayerPos = GameManager.instance.player.transform.position;
        Vector3 myPos = transform.position;
        float diffX = Mathf.Abs(PlayerPos.x - myPos.x);
        float diffY = Mathf.Abs(PlayerPos.y - myPos.y);

        Vector3 playerDir = GameManager.instance.player.inputVec;
        float dirX = playerDir.x < 0 ? -1 : 1;
        float dirY = playerDir.y < 0 ? -1 : 1;

        switch (transform.tag)
        {
            case "Ground":
                if (diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirX * MapConfig.instance.mapXSize * 2);
                }
                else if (diffX < diffY)
                {
                    transform.Translate(Vector3.up * dirY * MapConfig.instance.mapYSize * 2);
                }
                    break;
            case "Enemy":
                if (coll.enabled)
                {
                    transform.Translate(playerDir * MapConfig.instance.mapXSize + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0));
                }
                break;
        }
    }
}
