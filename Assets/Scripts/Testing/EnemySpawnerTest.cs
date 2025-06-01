using System;
using UnityEngine;
using VContainer;

namespace TestProject_Factura
{
    public class EnemySpawnerTest : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        
        private EnemySpawner enemySpawner;
        
        [Inject]
        private void Construct(EnemySpawner spawner)
        {
            enemySpawner = spawner;
        }
        
        private void Start()
        {
            if (enemySpawner == null)
            {
                Debug.LogError("EnemySpawner not injected!");
                return;
            }
            
            if (playerTransform == null)
            {
                Debug.LogError("Player transform not assigned!");
                return;
            }
            
            // Тестуємо спавн ворогів
            TestEnemySpawning();
        }
        
        private void TestEnemySpawning()
        {
            // Запускаємо спавн ворогів
            enemySpawner.SpawnEnemies(playerTransform);
            Debug.Log("<color=green>EnemySpawner Test: Started spawning enemies</color>");
        }
        
        // Метод для ручного тестування через Inspector
        public void TestToggleSpawning()
        {
            if (enemySpawner == null || playerTransform == null)
                return;
                
            // Запускаємо/зупиняємо спавн ворогів
            if (IsSpawning())
            {
                enemySpawner.StopSpawning();
                Debug.Log("<color=yellow>EnemySpawner Test: Stopped spawning enemies</color>");
            }
            else
            {
                enemySpawner.SpawnEnemies(playerTransform);
                Debug.Log("<color=green>EnemySpawner Test: Started spawning enemies</color>");
            }
        }
        
        // Допоміжний метод для перевірки стану спавнера
        private bool IsSpawning()
        {
            // Для тестових цілей, просто припускаємо, що спавнер активний, якщо є хоча б один ворог на сцені
            return GameObject.FindGameObjectsWithTag("Enemy").Length > 0;
        }
    }
} 