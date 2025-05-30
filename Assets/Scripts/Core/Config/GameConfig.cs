using System;
using UnityEngine;

namespace TestProject_Factura
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Prefabs")]
        public GameObject bulletPrefab;
        
        [Header("Level")]
        public float levelLength = 200f;
        public int enemyCount = 20;
        public Vector2 enemySpawnRangeX = new Vector2(-10f, 10f);
        
        [Header("Shooting")]
        public float bulletSpeed = 20f;
        public float bulletDamage = 25f;
        public float shootCooldown = 0.3f;
    }
} 