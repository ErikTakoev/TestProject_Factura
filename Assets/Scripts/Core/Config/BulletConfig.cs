using System;
using UnityEngine;

namespace TestProject_Factura
{
    [CreateAssetMenu(fileName = "BulletConfig", menuName = "Game/BulletConfig")]
    public class BulletConfig : ScriptableObject
    {
        [Header("Prefabs")]
        public GameObject bulletPrefab;
        public float bulletSpeed = 20f;
        public float bulletDamage = 25f;
        public float bulletLifetime = 0.4f;
        public int bulletCount = 10000;
        public float shootCooldown = 0.3f;
    }
}