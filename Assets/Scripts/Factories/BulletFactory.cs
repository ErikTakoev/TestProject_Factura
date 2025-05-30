using UnityEngine;
using VContainer;
using VContainer.Unity;
using System.Linq;

namespace TestProject_Factura
{
    public class BulletFactory
    {
        private readonly GameObject _bulletPrefab;
        
        [Inject]
        public BulletFactory(IObjectResolver container)
        {
            var prefabReferences = container.Resolve<PrefabReference[]>();
            _bulletPrefab = prefabReferences.FirstOrDefault(x => x.Key == "BulletPrefab")?.Prefab;
            
            if (_bulletPrefab == null)
            {
                Debug.LogError("BulletFactory: Не вдалося знайти префаб куль за ключем 'BulletPrefab'");
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