using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TestProject_Factura
{
    public class TurretController : MonoBehaviour, ITurretController
    {
        [Header("Rotation")]
        [SerializeField] private Transform turretPivot;
        [SerializeField] private float rotationSpeed = 100f;

        [Header("Shooting")]
        [SerializeField] private GameObject turretLight;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform parentForBullets;
        [SerializeField] private float bulletSpread = 50f; // Maximum bullet spread in degrees

        private int bulletCount;

        private Camera mainCamera;
        private BulletConfig bulletConfig;
        private ObjectPool<Bullet> bulletPool;
        private float lastShootTime;

        // Ссилка на InputManager
        private InputManager inputManager;

        public bool CanShoot => Time.time - lastShootTime >= bulletConfig.shootCooldown;

        [Inject]
        private void Construct(BulletConfig config, IObjectResolver resolver, InputManager inputMgr)
        {
            bulletConfig = config;
            inputManager = inputMgr;

            bulletCount = config.bulletCount;

            // Ініціалізуємо пул куль за допомогою Resolve
            if (bulletPrefab != null)
            {
                Bullet bulletComponent = bulletPrefab.GetComponent<Bullet>();
                if (bulletComponent != null)
                {
                    bulletPool = new ObjectPool<Bullet>(bulletComponent, parentForBullets, 1000, resolver);
                }
                else
                {
                    Debug.LogError("Bullet prefab does not have Bullet component!");
                }
            }
            else
            {
                Debug.LogError("Bullet prefab is not assigned!");
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;

            GameEvents.BulletCountChanged(bulletCount);
        }

        public void UpdateRotation(Vector2 inputPosition)
        {
            if (turretPivot == null)
                return;

            // Використовуємо InputManager для отримання світової позиції
            Vector3 worldPos = inputManager.GetWorldPosition();

            // Обчислюємо напрямок від турелі до цієї позиції
            Vector3 direction = worldPos - turretPivot.position;
            direction.y = 0;

            if (direction.magnitude < 0.1f)
                return;

            // Розраховуємо цільове обертання для осі Y
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

            // Плавно обертаємо турель до цільової орієнтації
            turretPivot.rotation = Quaternion.Slerp(
                turretPivot.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        public async UniTask Shoot()
        {
            GameStatistics.IncrementUniTaskCreated();
            if (!CanShoot || shootPoint == null || bulletPool == null)
                return;

            lastShootTime = Time.time;

            if (bulletCount <= 0)
                return;

            // Створюємо кулю з пулу
            Bullet bullet = bulletPool.Get();
            if (bullet != null)
            {
                bulletCount--;
                GameStatistics.IncrementBulletsFired();
                GameStatistics.IncrementParticlesCreated();
                GameEvents.BulletCountChanged(bulletCount);

                turretLight.SetActive(true);
                bullet.transform.position = shootPoint.position;

                // Встановлюємо напрямок кулі у горизонтальній площині
                Vector3 bulletDirection = turretPivot.forward;
                bulletDirection.y = 0; // Обмежуємо рух тільки в горизонтальній площині

                // Add random spread to bullet direction
                float randomSpreadX = UnityEngine.Random.Range(-bulletSpread, bulletSpread);
                bulletDirection = Quaternion.Euler(0, randomSpreadX, 0) * bulletDirection;

                bulletDirection = bulletDirection.normalized; // Нормалізуємо вектор

                // Встановлюємо обертання кулі відповідно до напрямку
                bullet.transform.rotation = Quaternion.LookRotation(bulletDirection);

                // Ініціалізуємо кулю з правильним напрямком
                bullet.Initialize(bulletDirection, bulletConfig.bulletSpeed, bulletConfig.bulletDamage, bulletPool);
            }

            await UniTask.Yield();
            turretLight.SetActive(false);
        }
    }
}