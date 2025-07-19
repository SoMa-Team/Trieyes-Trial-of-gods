using System;

namespace AttackSystem
{
    /// <summary>
    /// Attack Component ID 열거형
    /// 모든 AC들의 ID를 중앙에서 관리합니다.
    /// </summary>
    public enum AttackComponentID
    {
        // 기본 공격 컴포넌트
        AC001_HeroSword = 1,
        AC002_HeroSwordRadius = 2,
        
        // 무기 속성 부여 컴포넌트
        AC003_HeroSwordEnchantmentFire = 3,
        AC004_HeroSwordEnchantmentIce = 4,
        AC005_HeroSwordEnchantmentLightning = 5,
        AC006_HeroSwordEnchantmentHeaven = 6,
        
        // 무기 속성 연계 컴포넌트
        AC007_HeroFireMeteor = 7,
        AC008_IceStorm = 8,
        AC009_LightningField = 9,
        
        // AOE 및 DOT 컴포넌트
        AC100_AOE = 10,
        AC101_DOT = 11,
        
        // 특수 공격 컴포넌트
        AC102_CHAIN = 12,
        AC103_FALL = 13,
        AC104_GLOBAL = 14,
        AC105_FollowingField = 15,
    }
} 