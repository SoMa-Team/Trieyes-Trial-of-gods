using CharacterSystem;
using UnityEngine.InputSystem.Controls;
using System;
using UnityEngine;

namespace Stats
{
    public static class StatValueCalculator
    {
        public static float StatValueCalculate(Pawn pawn, StatType statType)
        {
            switch (statType)
            {
                case StatType.AttackPower:
                    return CalculateAttackPowerValue(pawn);
                case StatType.AttackSpeed:
                    return CalculateAttackSpeed(pawn);
                case StatType.GoldDropRate:
                    return CalculateGoldDrop(pawn);
                case StatType.ItemMagnet:
                    return CalculateMagnetLevel(pawn);
                case StatType.MoveSpeed:
                    return CalculateSpeed(pawn);
                case StatType.Reflect:
                    return CalculateReflection(pawn);
                case StatType.CriticalDamage:
                    return CalculateCriticalDamageValue(pawn);
                case StatType.CriticalRate:
                    return CalculateCriticalRateValue(pawn);
                case StatType.Health:
                    return CalculateHealth(pawn);
                case StatType.SkillCooldownReduction:
                    return CalculateSkillCooldownReduction(pawn);
                case StatType.HealthRegen:
                    return CalculateHealthRegen(pawn);
                case StatType.LifeSteal:
                    return CalculateLifeSteal(pawn);
                case StatType.Evasion:
                    return CalculateEvasion(pawn);
                default:
                    return pawn.statSheet[statType].Value;
            }
        }

        public static float CalculateAttackPowerValue(Pawn pawn)
        {
            return pawn.statSheet[StatType.AttackPower].Value;
        }

        public static float CalculateCriticalRateValue(Pawn pawn)
        {
            return pawn.statSheet[StatType.CriticalRate].Value;
        }

        public static float CalculateCriticalDamageValue(Pawn pawn)
        {
            return pawn.statSheet[StatType.CriticalDamage].Value;
        }

        public static float CalculateAttackSpeed(Pawn pawn)
        {
            const float multiplier = 1f/100;
            return Mathf.Clamp((float)pawn.statSheet[StatType.AttackSpeed].Value*multiplier, 0.01f, 10f);
        }

        public static float CalculateSkillCooldownReduction(Pawn pawn)
        {
            const float multiplier = 1000;
            return Mathf.Clamp(multiplier/(multiplier+(float)pawn.statSheet[StatType.SkillCooldownReduction].Value), 0.25f, 1f);
        }

        public static float CalculateReflection(Pawn pawn)
        {
            return pawn.statSheet[StatType.Reflect].Value;
        }

        public static float CalculateHealth(Pawn pawn)
        {
            return pawn.statSheet[StatType.Health].Value;
        }

        public static float CalculateHealthRegen(Pawn pawn)
        {
            return Mathf.Clamp((float)pawn.statSheet[StatType.HealthRegen].Value, 0f, 100f);
        }

        public static float CalculateLifeSteal(Pawn pawn)
        {
            const float multiplier = 1f/100;
            return Mathf.Clamp((float)pawn.statSheet[StatType.LifeSteal].Value*multiplier, 0f, 10f);
        }

        public static float CalculateDefense(Pawn pawn)
        {
            const float multiplier = 1000;
            return Mathf.Clamp(multiplier / (multiplier + (float)pawn.statSheet[StatType.Defense].Value), 0.1f, 1f);
        }

        public static float CalculateEvasion(Pawn pawn)
        {
            return pawn.statSheet[StatType.Evasion].Value;
        }

        public static float CalculateSpeed(Pawn pawn)
        {
            const float sensitivity = 0.01f;
            const float multiplier = 1f;
            const float minSpeed = 0;
            const float maxSpeed = 20;
            float logValue = Mathf.Log(1f + Mathf.Max((float)pawn.statSheet[StatType.MoveSpeed].Value*sensitivity, 0f));
            float M = multiplier * logValue;
            float result = Mathf.Clamp(M, minSpeed, maxSpeed);
            return result;
        }

        public static float CalculateMagnetLevel(Pawn pawn)
        {
            return pawn.statSheet[StatType.ItemMagnet].Value;
        }

        public static float CalculateGoldDrop(Pawn pawn)
        {
            return Mathf.Clamp(pawn.statSheet[StatType.GoldDropRate].Value, 0f, 100f);
        }
    }
}