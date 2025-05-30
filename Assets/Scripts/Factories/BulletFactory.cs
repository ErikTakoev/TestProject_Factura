using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TestProject_Factura
{
    public class BulletFactory
    {
        private readonly GameObject _bulletPrefab;
        private readonly GameConfig _gameConfig;
        
        [Inject]
        public BulletFactory(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
            _bulletPrefab = _gameConfig.bulletPrefab;
            
            if (_bulletPrefab == null)
            {
                Debug.LogError("BulletFactory: Префаб куль не налаштовано в GameConfig");
            }
        }
        
        public GameObject CreateBullet(Vector3 position, Quaternion rotation)
        {
            if (_bulletPrefab == null)
            {
                Debug.LogError("BulletFactory: Префаб куль не знайдено");
                return null;
            }
            
            return Object.Instantiate(_bulletPrefab, position, rotation);
        }
    }
} 