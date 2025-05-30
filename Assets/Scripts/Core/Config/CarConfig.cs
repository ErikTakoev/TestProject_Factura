using System;
using UnityEngine;

namespace TestProject_Factura
{
    [CreateAssetMenu(fileName = "CarConfig", menuName = "Game/CarConfig")]
    public class CarConfig : ScriptableObject
    {
        [Header("Movement")]
        public float moveSpeed = 10f;
        
        [Header("Health")]
        public float maxHP = 100f;
        
        [Header("Physics")]
        public float acceleration = 5f;
    }
} 