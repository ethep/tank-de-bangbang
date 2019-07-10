

using UnityEngine;

public static class LevelDesign
{
    public const float GameTime = 30f;
    public const int LevelMax = 5;
    public const int DefeatTankPoint = 100;
    public const int DefeatBponusPoint = 1000;

    public static int RandomSeed
    {
        get { return PlayerPrefs.GetInt("random"); }
        set { PlayerPrefs.SetInt("random", value); }
    }

    public static class Player
    {
        public const float ShellSpeedMin = 10f;
        public const float ShellSpeedMax = 50f;
        public const float TankSpeedMin = 200f;
        public const float TankSpeedMax = 500f;
        public const float FireRateMin = 4.0f;
        public const float FireRateMax = 0.2f;

        public static int ShellSpeedLevel
        {
            get { return PlayerPrefs.GetInt("shell_speed"); }
            set { PlayerPrefs.SetInt("shell_speed", value <= LevelMax ? value : LevelMax); }
        }

        public static int TankSpeedLevel
        {
            get { return PlayerPrefs.GetInt("tank_speed"); }
            set { PlayerPrefs.SetInt("tank_speed", value <= LevelMax ? value : LevelMax); }
        }

        public static int FireRateLevel
        {
            get { return PlayerPrefs.GetInt("fire_rate"); }
            set { PlayerPrefs.SetInt("fire_rate", value <= LevelMax ? value : LevelMax); }
        }

        public static float ShellSpeed()
        {
            var rate = ShellSpeedLevel / LevelMax;
            return Mathf.Lerp(ShellSpeedMin, ShellSpeedMax, rate);
        }

        public static float TankSpeed()
        {
            var rate = ShellSpeedLevel / LevelMax;
            return Mathf.Lerp(TankSpeedMin, TankSpeedMax, rate);
        }

        public static float FireRate()
        {
            var rate = ShellSpeedLevel / LevelMax;
            return Mathf.Lerp(FireRateMin, FireRateMax, rate);
        }
    }

    public static class Enemy
    {
        public const float ShellSpeedMin = 5f;
        public const float ShellSpeedMax = 10f;
        public const float TankSpeedMin = 100f;
        public const float TankSpeedMax = 300f;
        public const float FireRateMin = 7f;
        public const float FireRateMax = 1.5f;

        // 出現速度
        public const float SpawnIntervalMin = 13f;
        public const float SpawnIntervalMax = 2f;
        // 強力な個体が出現するようになるレベル
        public const int StrongSpawnLevel = 5;
        public const int StrongSpawnRate = 30;
        // ボーナスが出現する撃破数
        public const int BonusRequiredDefeat = 1;
        // ボーナスは固定速度
        public const float BonusSpeed = 100f;

        public static float Rate { get { return (float)GameManager.Instance.GameLevel.Value / (float)LevelMax; } }

        public static float ShellSpeed()
        {
            return Mathf.Lerp(ShellSpeedMin, ShellSpeedMax, Rate);
        }

        public static float TankSpeed()
        {
            return Mathf.Lerp(TankSpeedMin, TankSpeedMax, Rate);
        }

        public static float FireRate()
        {
            return Mathf.Lerp(FireRateMin, FireRateMax, Rate);
        }

        public static float SpawnInterval()
        {
            return Mathf.Lerp(SpawnIntervalMin, SpawnIntervalMax, Rate);
        }

        public static bool LotteryStrongSpawn()
        {
            return GameManager.Instance.GameLevel.Value > StrongSpawnLevel && Random.Range(0, StrongSpawnRate) == 0;
        }
    }
}
