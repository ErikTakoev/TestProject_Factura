using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using VContainer;

namespace TestProject_Factura
{
    public class EnemyController : MonoBehaviour, IEnemyController
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float deathEffectDuration = 1.5f;
        [SerializeField] private ParticleSystem hitEffect;
        [SerializeField] private ParticleSystem deathEffect;
        [SerializeField] private float rotationSpeed = 5f; // Швидкість обертання до цілі
        [SerializeField] private float sendToPoolIfBackward = -12f;
        
        // Компоненти
        private Rigidbody rb;
        
        // Конфігурація
        private EnemyConfig config;
        private float currentHP;
        private Transform target;
        private bool isActive = false;
        
        // State Machine
        private Dictionary<EnemyStateType, IEnemyState> states;
        private IEnemyState currentState;
        
        // Властивості
        public Transform Target => target;
        public float DistanceToTarget => target != null ? Vector3.Distance(transform.position, target.position) : float.MaxValue;
        public EnemyConfig Config => config;
        
        [Inject]
        private void Construct(EnemyConfig enemyConfig)
        {
            config = enemyConfig;
        }
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }
        
        private void Start()
        {
            InitializeStateMachine();
            currentHP = config.maxHP;
        }
        
        private void Update()
        {
            if (!isActive || currentState == null)
                return;

            if (transform.position.z - target.position.z < sendToPoolIfBackward)
            {
                gameObject.SetActive(false);
                return;
            }
                
            currentState.Update();
        }
        
        private void InitializeStateMachine()
        {
            states = new Dictionary<EnemyStateType, IEnemyState>
            {
                { EnemyStateType.Idle, new IdleState(this) },
                { EnemyStateType.Chase, new ChaseState(this) },
                { EnemyStateType.Attack, new AttackState(this) }
            };
            
            ChangeState(EnemyStateType.Idle);
        }
        
        public void ChangeState(EnemyStateType stateType)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }
            
            currentState = states[stateType];
            currentState.Enter();
        }
        
        public void Initialize(Vector3 spawnPosition)
        {
            transform.position = spawnPosition;
            currentHP = config.maxHP;
            isActive = true;
            
            
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
                // Встановлюємо обмеження на обертання для запобігання падіння
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
            
            gameObject.SetActive(true);
            InitializeStateMachine();
        }
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        // Метод для повороту ворога в напрямку цілі
        public void LookAtTarget()
        {
            if (target == null)
                return;
                
            // Отримуємо напрямок до цілі
            Vector3 direction = (target.position - transform.position).normalized;
            
            // Ігноруємо Y-компонент для обертання тільки по горизонталі
            direction.y = 0;
            
            // Перевіряємо, що вектор не нульовий (щоб уникнути помилок)
            if (direction != Vector3.zero)
            {
                // Створюємо кватерніон повороту до цілі
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                
                // Плавно повертаємося до цілі
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
        }
        
        public async UniTask TakeDamage(float damage)
        {
            if (damage <= 0 || !isActive)
                return;
                
            currentHP = Mathf.Max(0, currentHP - damage);
            
            // Відтворюємо ефект влучання
            if (hitEffect != null)
            {
                hitEffect.Play();
            }
            
            // Якщо HP закінчилося, знищуємо ворога
            if (currentHP <= 0)
            {
                await Die();
            }
        }
        
        private async UniTask Die()
        {
            isActive = false;
            
            // Зупиняємо стан
            if (currentState != null)
            {
                currentState.Exit();
                currentState = null;
            }
            
            
            if (rb != null)
            {
                // Спочатку зупиняємо рух
                if (!rb.isKinematic)
                {
                    rb.velocity = Vector3.zero;
                }
                // Потім переключаємо в кінематичний режим
                rb.isKinematic = true;
            }
            
            // Відтворюємо ефект смерті
            if (deathEffect != null)
            {
                deathEffect.Play();
            }
            
            // Анімація смерті
            SetAnimation("Die");
            
            // Чекаємо завершення анімації смерті
            await UniTask.Delay(TimeSpan.FromSeconds(deathEffectDuration));
            
            // Сповіщаємо про вбивство ворога
            GameEvents.EnemyKilled(1);
            
            // Вимикаємо об'єкт
            gameObject.SetActive(false);
        }
        
        public void SetAnimation(string animationName)
        {
            if (animator != null)
            {
                animator.SetTrigger(animationName);
            }
        }
    }
} 