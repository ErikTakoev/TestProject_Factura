using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TestProject_Factura
{
    public class EnemyFactory
    {
        private readonly GameObject _enemyPrefab;
        private readonly EnemyConfig _enemyConfig;
        
        [Inject]
        public EnemyFactory(EnemyConfig enemyConfig)
        {
            _enemyConfig = enemyConfig;
            _enemyPrefab = _enemyConfig.enemyPrefab;
            
            if (_enemyPrefab == null)
            {
                Debug.LogError("EnemyFactory: Префаб ворога не налаштовано в EnemyConfig");
            }
        }
        
        public GameObject CreateEnemy(Vector3 position)
        {
            if (_enemyPrefab == null)
            {
                Debug.LogError("EnemyFactory: Префаб ворога не знайдено");
                return null;
            }
            
            GameObject enemy = Object.Instantiate(_enemyPrefab, position, Quaternion.identity);
            
            // Тут можна налаштувати ворога згідно з конфігурацією
            // Наприклад, встановити швидкість, здоров'я тощо
            
            return enemy;
        }
    }
} 