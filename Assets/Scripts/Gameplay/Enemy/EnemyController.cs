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
        [SerializeField] private GameObject model;
        [SerializeField] private SphereCollider sphereCollider;
        [SerializeField] private float deathEffectDuration = 1.5f;
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

        // Пул об'єктів
        private ObjectPool<EnemyController> pool;

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
                ReturnToPool();
                return;
            }

            // Викликаємо Update стану для нефізичних операцій
            currentState.Update();
        }

        private void FixedUpdate()
        {
            if (!isActive || currentState == null)
                return;

            currentState.FixedUpdate();
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

        // Reset particle systems to their original state
        private void ResetParticleSystems()
        {
            if (deathEffect != null)
            {
                // Reset main module properties
                var main = deathEffect.main;
                main.gravityModifier = 0;
            }
        }

        public void Initialize(Vector3 spawnPosition, ObjectPool<EnemyController> objectPool = null)
        {
            transform.position = spawnPosition;
            currentHP = config.maxHP;
            isActive = true;

            // Reset particle systems to original state
            ResetParticleSystems();

            // Встановлюємо посилання на пул
            if (objectPool != null)
            {
                pool = objectPool;
            }

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
            }
            model.transform.localEulerAngles = Vector3.zero;
            model.SetActive(true);
            sphereCollider.enabled = true;

            gameObject.SetActive(true);
            InitializeStateMachine();
        }

        // Метод для встановлення посилання на пул об'єктів
        public void SetPool(ObjectPool<EnemyController> objectPool)
        {
            pool = objectPool;
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

            model.SetActive(false);
            sphereCollider.enabled = false;

            // Start with current color
            Color startColor = Color.red;

            // Create random target color
            Color endColor = new Color(
                UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(0f, 1f),
                startColor.a * 0.3f // Fade out
            );

            deathEffect.SetGradient(new GradientColorKey[] {
                        new GradientColorKey(startColor, 0.3f),
                        new GradientColorKey(endColor, 0.4f)
                },
                new GradientAlphaKey[] {
                        new GradientAlphaKey(startColor.a, 0.3f),
                        new GradientAlphaKey(endColor.a, 0.4f)
                }
            );


            deathEffect.Simulate(1.5f, true, true, true);
            deathEffect.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));

            var main = deathEffect.main;
            main.gravityModifier = 1f;
            var particles = new ParticleSystem.Particle[deathEffect.main.maxParticles];
            int count = deathEffect.GetParticles(particles);

            for (int i = 0; i < count; i++)
            {
                // Випадкова швидкість у всі боки
                particles[i].velocity = UnityEngine.Random.onUnitSphere * UnityEngine.Random.Range(1f, 3f);
            }

            deathEffect.SetParticles(particles, count);

            // Чекаємо завершення анімації смерті
            await UniTask.Delay(TimeSpan.FromSeconds(deathEffectDuration));

            // Сповіщаємо про вбивство ворога
            GameEvents.EnemyKilled(1);

            // Повертаємо об'єкт в пул замість простої деактивації
            ReturnToPool();
        }

        // Метод для повернення об'єкта до пулу
        private void ReturnToPool()
        {
            if (pool != null)
            {
                pool.Return(this);
            }
            else
            {
                // Якщо пул не встановлено, просто деактивуємо об'єкт
                gameObject.SetActive(false);
            }
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