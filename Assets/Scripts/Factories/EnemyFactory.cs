using UnityEngine;
using VContainer;
using VContainer.Unity;
using System.Linq;

namespace TestProject_Factura
{
    public class EnemyFactory
    {
        private readonly GameObject _enemyPrefab;
        private readonly EnemyConfig _enemyConfig;
        
        [Inject]
        public EnemyFactory(IObjectResolver container, EnemyConfig enemyConfig)
        {
            var prefabReferences = container.Resolve<PrefabReference[]>();
            _enemyPrefab = prefabReferences.FirstOrDefault(x => x.Key == "EnemyPrefab")?.Prefab;
            _enemyConfig = enemyConfig;
            
            if (_enemyPrefab == null)
            {
                Debug.LogError("EnemyFactory: Не вдалося знайти префаб ворога за ключем 'EnemyPrefab'");
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