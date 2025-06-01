using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TestProject_Factura
{
    public class EnemyFactory
    {
        private readonly GameObject enemyPrefab;
        private readonly EnemyConfig enemyConfig;

        [Inject]
        public EnemyFactory(EnemyConfig enemyConfig)
        {
            this.enemyConfig = enemyConfig;
            enemyPrefab = this.enemyConfig.enemyPrefab;

            if (enemyPrefab == null)
            {
                Debug.LogError("EnemyFactory: Префаб ворога не налаштовано в EnemyConfig");
            }
        }

        public GameObject CreateEnemy(Vector3 position)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("EnemyFactory: Префаб ворога не знайдено");
                return null;
            }

            GameObject enemy = Object.Instantiate(enemyPrefab, position, Quaternion.identity);

            return enemy;
        }
    }
}