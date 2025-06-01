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

        [Header("Steering")]
        [Tooltip("Coefficient that determines how sharply the car turns based on its X position")]
        public float steeringFactor = 5f;
        [Tooltip("Speed at which the car returns to the center path")]
        public float returnSpeed = 2f;
    }
}