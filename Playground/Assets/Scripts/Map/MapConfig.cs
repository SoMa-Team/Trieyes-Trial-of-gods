using UnityEngine;

public class MapConfig : MonoBehaviour
{
    public int mapXSize;
    public int mapYSize;
    public static MapConfig instance;
    private void Awake()
    {
        instance = this;
    }
}
