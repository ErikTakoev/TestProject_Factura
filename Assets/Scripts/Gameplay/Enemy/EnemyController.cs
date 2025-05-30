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
            }
            
            gameObject.SetActive(true);
            ChangeState(EnemyStateType.Idle);
        }
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
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
        
        private void OnTriggerEnter(Collider other)
        {
            // Логіка для виявлення зіткнень з кулями буде реалізована пізніше
        }
    }
} 