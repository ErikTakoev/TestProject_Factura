using System;
using UnityEngine;
using VContainer;

namespace TestProject_Factura
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private Transform groundPrefab;
        [SerializeField] private int groundTilesCount = 3;
        [SerializeField] private float groundTileLength = 100f;
        
        private Transform[] groundTiles;
        private Transform carTransform;
        private int nextTileIndex = 0;
        private float resetThreshold;
        
        [Inject]
        private void Construct(ICarController carController)
        {
            if (carController != null)
            {
                carTransform = carController.Transform;
            }
        }
        
        private void Start()
        {
            if (groundPrefab == null)
            {
                Debug.LogError("Ground prefab is not assigned!");
                return;
            }
            
            // Створюємо початкові тайли землі
            InitializeGroundTiles();
            
            // Встановлюємо поріг для переміщення тайлу
            resetThreshold = groundTileLength * 0.6f;
        }
        
        private void Update()
        {
            if (carTransform == null || groundTiles == null || groundTiles.Length == 0)
                return;
                
            // Перевіряємо, чи проїхав автомобіль достатньо далеко для переміщення тайлу
            float carZ = carTransform.position.z;
            
            // Отримуємо перший тайл (який автомобіль вже проїхав)
            Transform firstTile = groundTiles[nextTileIndex];
            
            // Якщо автомобіль проїхав достатньо далеко від першого тайлу
            if (carZ - firstTile.position.z > resetThreshold)
            {
                // Переміщуємо тайл вперед
                MoveGroundTileForward(firstTile);
                
                // Оновлюємо індекс наступного тайлу
                nextTileIndex = (nextTileIndex + 1) % groundTiles.Length;
            }
        }
        
        private void InitializeGroundTiles()
        {
            groundTiles = new Transform[groundTilesCount];
            
            for (int i = 0; i < groundTilesCount; i++)
            {
                // Створюємо тайл землі
                Transform tile = Instantiate(groundPrefab, transform);
                
                // Позиціонуємо тайл
                tile.position = new Vector3(0, 0, i * groundTileLength);
                
                // Зберігаємо посилання на тайл
                groundTiles[i] = tile;
            }
        }
        
        private void MoveGroundTileForward(Transform tile)
        {
            // Знаходимо позицію останнього тайлу
            float lastTileZ = 0f;
            
            for (int i = 0; i < groundTiles.Length; i++)
            {
                if (i != nextTileIndex && groundTiles[i].position.z > lastTileZ)
                {
                    lastTileZ = groundTiles[i].position.z;
                }
            }
            
            // Переміщуємо тайл вперед
            Vector3 newPosition = tile.position;
            newPosition.z = lastTileZ + groundTileLength;
            tile.position = newPosition;
        }
    }
} 