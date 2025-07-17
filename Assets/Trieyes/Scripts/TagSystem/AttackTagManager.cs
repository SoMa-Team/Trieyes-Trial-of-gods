using System;
using RelicSystem;

namespace TagSystem
{
    public class AttackTagManager
    {
        public static AttackTag? GetAttackTagByName(string tagString)
        {
            // TODO: Tag종류에 따라 수정 필요
            return tagString switch
            {
                "fire" => AttackTag.Fire,
                "water" => AttackTag.Water,
                "earth" => AttackTag.Earth,
                "light" => AttackTag.Light,
                "dark" => AttackTag.Dark,
                "range" => AttackTag.Range,
                _ => throw new Exception("Unknown tag string: " + tagString)
            };
        }

        public static bool isValidRelicStat(RelicStatType relicStatType)
        {
            throw new NotImplementedException();
        }
    }
}