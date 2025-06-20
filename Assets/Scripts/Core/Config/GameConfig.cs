using System;
using UnityEngine;

namespace TestProject_Factura
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
    public class GameConfig : ScriptableObject
    {

        [Header("Level")]
        public float levelLength = 200f;
        public int enemyCount = 20;

        [Header("Enemy Spawn")]
        public float spawnInterval = 2f;
        public Vector2 enemySpawnRangeX = new Vector2(-10f, 10f);
        public Vector2 enemySpawnRangeY = new Vector2(40f, 100f);
    }
}