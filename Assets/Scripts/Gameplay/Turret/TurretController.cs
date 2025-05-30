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
        [SerializeField] private Vector2 rotationLimits = new Vector2(-90f, 90f);
        
        [Header("Shooting")]
        [SerializeField] private Transform shootPoint;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private AudioSource shootSound;
        
        private Camera mainCamera;
        private GameConfig gameConfig;
        private ObjectPool<Bullet> bulletPool;
        private float lastShootTime;
        
        public bool CanShoot => Time.time - lastShootTime >= gameConfig.shootCooldown;
        
        [Inject]
        private void Construct(GameConfig config, IObjectResolver resolver)
        {
            gameConfig = config;
            
            // Ініціалізуємо пул куль за допомогою Resolve
            if (bulletPrefab != null)
            {
                Bullet bulletComponent = bulletPrefab.GetComponent<Bullet>();
                if (bulletComponent != null)
                {
                    bulletPool = new ObjectPool<Bullet>(bulletComponent, transform, 10, resolver);
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
            if (turretPivot == null || mainCamera == null)
                return;
                
            // Отримуємо позицію у світових координатах з вхідного вектора
            Vector3 worldPos = GetWorldPointFromInput(inputPosition);
            
            // Обчислюємо напрямок від турелі до цієї позиції (тільки в горизонтальній площині XZ)
            Vector3 direction = worldPos - turretPivot.position;
            direction.y = 0; // Обмежуємо обертання тільки по горизонталі
            
            if (direction.magnitude < 0.1f)
                return;
                
            // Розраховуємо цільове обертання
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            
            // Обмежуємо кут обертання, якщо необхідно
            float currentAngle = ClampRotationAngle(targetRotation.eulerAngles.y);
            
            // Плавно обертаємо турель до цільової орієнтації
            turretPivot.rotation = Quaternion.RotateTowards(
                turretPivot.rotation,
                Quaternion.Euler(0, currentAngle, 0),
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
                bullet.transform.position = shootPoint.position;
                bullet.transform.rotation = shootPoint.rotation;
                bullet.Initialize(shootPoint.forward, gameConfig.bulletSpeed, gameConfig.bulletDamage, bulletPool);
            }
            
            await UniTask.Yield();
        }
        
        private Vector3 GetWorldPointFromInput(Vector2 screenPoint)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPoint);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            
            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }
            
            return Vector3.zero;
        }
        
        private float ClampRotationAngle(float angle)
        {
            // Нормалізуємо кут до діапазону [0, 360)
            if (angle > 180)
                angle -= 360;
                
            // Обмежуємо кут між мінімальним і максимальним значеннями
            return Mathf.Clamp(angle, rotationLimits.x, rotationLimits.y);
        }
    }
} 