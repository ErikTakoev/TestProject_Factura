using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace TestProject_Factura
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform levelParent;
        [SerializeField] private float spawnInterval = 2f; // Інтервал між спавном ворогів
        
        private GameConfig gameConfig;
        private ObjectPool<EnemyController> enemyPool;
        private Transform carTransform;
        private bool isSpawning = false;
        private int spawnedEnemies = 0;
        private int enemiesRemaining = 0;
        private float lastSpawnTime = 0f;
        private IObjectResolver container;
        
        [Inject]
        private void Construct(GameConfig config, IObjectResolver resolver)
        {
            gameConfig = config;
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
            if (Time.time - lastSpawnTime > spawnInterval && spawnedEnemies < gameConfig.enemyCount)
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
                // GameEvents.LevelCompleted();
            }
        }
        
        private void SpawnEnemy()
        {
            if (enemyPool == null || carTransform == null)
                return;
                
            // Отримуємо ворога з пулу
            EnemyController enemy = enemyPool.Get();
            
            if (enemy != null)
            {
                // Генеруємо випадкову позицію для спавну
                Vector3 spawnPosition = GetRandomSpawnPosition(carTransform.position.z + UnityEngine.Random.Range(10f, 30f));
                
                // Ініціалізуємо ворога
                enemy.Initialize(spawnPosition);
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
            if (enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab is not assigned!");
                return;
            }
            
            EnemyController enemyComponent = enemyPrefab.GetComponent<EnemyController>();
            if (enemyComponent == null)
            {
                Debug.LogError("Enemy prefab does not have EnemyController component!");
                return;
            }
            
            enemyPool = new ObjectPool<EnemyController>(enemyComponent, transform, 10, container);
        }
    }
} 