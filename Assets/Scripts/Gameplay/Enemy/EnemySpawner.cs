using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace TestProject_Factura
{
    public class EnemySpawner : MonoBehaviour
    {
        private GameConfig gameConfig;
        private EnemyConfig enemyConfig;
        private ObjectPool<EnemyController> enemyPool;
        private Transform carTransform;
        private bool isSpawning = false;
        private int spawnedEnemies = 0;
        private int enemiesRemaining = 0;
        private float lastSpawnTime = 0f;
        private IObjectResolver container;

        [Inject]
        private void Construct(GameConfig config, EnemyConfig enemyConfig, IObjectResolver resolver)
        {
            gameConfig = config;
            this.enemyConfig = enemyConfig;
            container = resolver;
        }

        private void Start()
        {
            InitializeObjectPool();

            // Підписуємося на подію знищення ворога
            GameEvents.OnEnemyKilled += OnEnemyKilled;
        }

        private void OnDestroy()
        {
            // Відписуємося від події
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
        }

        private void Update()
        {
            if (!isSpawning || carTransform == null)
                return;

            // Спавнимо ворогів з інтервалом
            if (Time.time - lastSpawnTime > gameConfig.spawnInterval && spawnedEnemies < gameConfig.enemyCount)
            {
                SpawnEnemy();
                lastSpawnTime = Time.time;
            }
        }

        public void SpawnEnemies(Transform target)
        {
            if (target == null)
                return;

            carTransform = target;
            spawnedEnemies = 0;
            enemiesRemaining = gameConfig.enemyCount;
            isSpawning = true;
            lastSpawnTime = Time.time;
        }

        public void StopSpawning()
        {
            isSpawning = false;
        }

        private void OnEnemyKilled(int count)
        {
            enemiesRemaining -= count;

            // Якщо всі вороги знищені, відправляємо подію про перемогу
            if (enemiesRemaining <= 0)
            {
                GameEvents.LevelCompleted();
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPool == null || carTransform == null)
                return;
            Vector3 spawnPosition = GetRandomSpawnPosition(carTransform.position.z + UnityEngine.Random.Range(gameConfig.enemySpawnRangeY.x, gameConfig.enemySpawnRangeY.y));
            if (spawnPosition.z > gameConfig.levelLength)
                return;

            // Отримуємо ворога з пулу
            EnemyController enemy = enemyPool.Get();

            if (enemy != null)
            {
                // Генеруємо випадкову позицію для спавну

                // Ініціалізуємо ворога, передаючи посилання на пул
                enemy.Initialize(spawnPosition, enemyPool);
                enemy.SetTarget(carTransform);

                spawnedEnemies++;
            }
        }

        private Vector3 GetRandomSpawnPosition(float zPosition)
        {
            float xPosition = UnityEngine.Random.Range(gameConfig.enemySpawnRangeX.x, gameConfig.enemySpawnRangeX.y);
            return new Vector3(xPosition, 0f, zPosition);
        }

        private void InitializeObjectPool()
        {
            if (enemyConfig.enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab is not assigned in EnemyConfig!");
                return;
            }

            EnemyController enemyComponent = enemyConfig.enemyPrefab.GetComponent<EnemyController>();
            if (enemyComponent == null)
            {
                Debug.LogError("Enemy prefab in EnemyConfig does not have EnemyController component!");
                return;
            }

            enemyPool = new ObjectPool<EnemyController>(enemyComponent, transform, 20, container);
        }
    }
}