using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace TestProject_Factura
{
    public class CarController : MonoBehaviour, ICarController
    {
        [SerializeField] private Rigidbody rb;
        
        private CarConfig config;
        private float currentHP;
        private Vector3 initialPosition;
        
        public Transform Transform => transform;
        public float CurrentHP => currentHP;
        public bool IsMoving { get; private set; }
        
        [Inject]
        private void Construct(CarConfig carConfig)
        {
            config = carConfig;
        }
        
        private void Awake()
        {
            // Перевіряємо, чи є компонент Rigidbody
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
            
            // Зберігаємо початкову позицію для рестарту
            initialPosition = transform.position;
        }
        
        private void Start()
        {
            // Ініціалізуємо HP
            currentHP = config.maxHP;
        }
        
        private void OnEnable()
        {
            // Встановлюємо початкові значення
            currentHP = config.maxHP;
            IsMoving = false;
            
            // Повертаємо автомобіль в початкову позицію
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                transform.position = initialPosition;
                transform.rotation = Quaternion.identity;
            }
            
            // Сповіщаємо UI про оновлення HP
            GameEvents.CarHPChanged(currentHP);
        }
        
        private void FixedUpdate()
        {
            if (IsMoving && rb != null)
            {
                // Отримуємо поточну позицію по X
                var posX = transform.position.x;
                
                // Корегуємо поворот автомобіля, щоб він повертався до центру шляху (X = 0)
                float steeringAngle = -posX * config.steeringFactor;
                Quaternion targetRotation = Quaternion.Euler(0, steeringAngle, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * config.returnSpeed);
                
                // Рухаємо автомобіль вперед з постійною швидкістю
                rb.velocity = Vector3.MoveTowards(rb.velocity, transform.forward * config.moveSpeed, Time.fixedDeltaTime * config.moveSpeed);
            }
        }

        public async UniTask StartMoving()
        {
            if (IsMoving || rb == null)
                return;

            IsMoving = true;

            // Плавно прискорюємо автомобіль
            float currentSpeed = 0f;
            float accelerationTime = 0f;

            while (currentSpeed < config.moveSpeed && accelerationTime < 1f)
            {
                accelerationTime += Time.deltaTime;
                currentSpeed = Mathf.Lerp(0, config.moveSpeed, accelerationTime * config.acceleration);
                rb.velocity = transform.forward * currentSpeed;
                await UniTask.Yield();
            }

            // Встановлюємо кінцеву швидкість
            rb.velocity = transform.forward * config.moveSpeed;
        }
        
        public void TakeDamage(float damage)
        {
            if (damage <= 0)
                return;
                
            currentHP = Mathf.Max(0, currentHP - damage);
            
            // Сповіщаємо UI про оновлення HP
            GameEvents.CarHPChanged(currentHP);
            
            // Якщо HP стало нуль, то сповіщаємо про знищення автомобіля
            if (currentHP <= 0)
            {
                Stop();
                GameEvents.CarDestroyed();
            }
        }
        
        public void Stop()
        {
            if (!IsMoving || rb == null)
                return;
                
            IsMoving = false;
            rb.velocity = Vector3.zero;
        }
    }
} 