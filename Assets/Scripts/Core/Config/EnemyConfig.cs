using System;
using UnityEngine;

namespace TestProject_Factura
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game/EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Health")]
        public float maxHP = 30f;
        
        [Header("Movement")]
        public float chaseSpeed = 8f;
        public float detectionRange = 15f;
        public float attackRange = 2f;
        
        [Header("Combat")]
        public float attackDamage = 10f;
        public float attackCooldown = 1f;
    }
} 