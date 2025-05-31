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
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private AudioSource shootSound;
        [SerializeField] private Transform parentForBullets;

        private Camera mainCamera;
        private GameConfig gameConfig;
        private ObjectPool<Bullet> bulletPool;
        private float lastShootTime;

        // Ссилка на InputManager
        private InputManager inputManager;

        public bool CanShoot => Time.time - lastShootTime >= gameConfig.shootCooldown;

        [Inject]
        private void Construct(GameConfig config, IObjectResolver resolver, InputManager inputMgr)
        {
            gameConfig = config;
            inputManager = inputMgr;

            // Ініціалізуємо пул куль за допомогою Resolve
            if (bulletPrefab != null)
            {
                Bullet bulletComponent = bulletPrefab.GetComponent<Bullet>();
                if (bulletComponent != null)
                {
                    bulletPool = new ObjectPool<Bullet>(bulletComponent, parentForBullets, 10, resolver);
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
            if (!CanShoot || shootPoint == null || bulletPool == null)
                return;

            lastShootTime = Time.time;

            // Відтворюємо ефект пострілу
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            // Відтворюємо звук пострілу
            if (shootSound != null)
            {
                shootSound.Play();
            }

            // Створюємо кулю з пулу
            Bullet bullet = bulletPool.Get();
            if (bullet != null)
            {
                turretLight.SetActive(true);
                bullet.transform.position = shootPoint.position;

                // Встановлюємо напрямок кулі у горизонтальній площині
                Vector3 bulletDirection = turretPivot.forward;
                bulletDirection.y = 0; // Обмежуємо рух тільки в горизонтальній площині
                bulletDirection = bulletDirection.normalized; // Нормалізуємо вектор

                // Встановлюємо обертання кулі відповідно до напрямку
                bullet.transform.rotation = Quaternion.LookRotation(bulletDirection);

                // Ініціалізуємо кулю з правильним напрямком
                bullet.Initialize(bulletDirection, gameConfig.bulletSpeed, gameConfig.bulletDamage, bulletPool);
            }

            await UniTask.Yield();
            turretLight.SetActive(false);
        }
    }
}