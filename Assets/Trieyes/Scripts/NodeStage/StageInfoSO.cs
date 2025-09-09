using UnityEngine;

namespace NodeStage
{
    public class StageInfoSO : ScriptableObject
    {
        public StageType type;
        public string name;
        public Sprite icon;

        public static StageInfoSO Create(StageType type, string name, Sprite icon)
        {
            var so = CreateInstance<StageInfoSO>();
            so.type = type;
            so.name = name;
            so.icon = icon;
            return so;
        }
    }
}