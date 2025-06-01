using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TestProject_Factura
{
    public class BulletFactory
    {
        private readonly GameObject bulletPrefab;
        private readonly BulletConfig bulletConfig;

        [Inject]
        public BulletFactory(BulletConfig bulletConfig)
        {
            this.bulletConfig = bulletConfig;
            bulletPrefab = this.bulletConfig.bulletPrefab;

            if (bulletPrefab == null)
            {
                Debug.LogError("BulletFactory: Префаб куль не налаштовано в BulletConfig");
            }
        }

        public GameObject CreateBullet(Vector3 position, Quaternion rotation)
        {
            if (bulletPrefab == null)
            {
                Debug.LogError("BulletFactory: Префаб куль не знайдено");
                return null;
            }

            return Object.Instantiate(bulletPrefab, position, rotation);
        }
    }
}