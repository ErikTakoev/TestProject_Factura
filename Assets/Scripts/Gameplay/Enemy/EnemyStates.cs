using System;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;

namespace TestProject_Factura
{
    // Базовий інтерфейс для станів ворога
    public interface IEnemyState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
    }
    
    // Базовий клас для всіх станів
    public abstract class EnemyStateBase : IEnemyState
    {
        protected readonly EnemyController enemy;
        
        protected EnemyStateBase(EnemyController enemy)
        {
            this.enemy = enemy;
        }
        
        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }
    
    // Стан спокою
    public class IdleState : EnemyStateBase
    {
        private float detectionCheckInterval = 0.5f;
        private float lastCheckTime;
        
        public IdleState(EnemyController enemy) : base(enemy) { }
        
        public override void Enter()
        {
            enemy.SetAnimation("Idle");
            lastCheckTime = Time.time;
        }
        
        public override void Update()
        {
            // Перевіряємо відстань до цілі через інтервал часу
            if (Time.time - lastCheckTime >= detectionCheckInterval)
            {
                lastCheckTime = Time.time;
                
                if (enemy.Target != null && enemy.DistanceToTarget <= enemy.Config.detectionRange)
                {
                    enemy.ChangeState(EnemyStateType.Chase);
                }
            }
        }
    }
    
    // Стан переслідування
    public class ChaseState : EnemyStateBase
    {
        private Rigidbody rb;
        
        public ChaseState(EnemyController enemy) : base(enemy)
        {
            rb = enemy.GetComponent<Rigidbody>();
        }
        
        public override void Enter()
        {
            enemy.SetAnimation("Run");
            
            // Переконуємося, що Rigidbody не кінематичний перед встановленням швидкості
            if (rb != null && rb.isKinematic)
            {
                rb.isKinematic = false;
            }
        }
        
        public override void Update()
        {
            if (enemy.Target == null)
            {
                enemy.ChangeState(EnemyStateType.Idle);
                return;
            }
            
            // Поворот до цілі - це можна залишити в Update
            enemy.LookAtTarget();
            
            // Перевіряємо відстань до цілі
            float distance = enemy.DistanceToTarget;
            
            if (distance <= enemy.Config.attackRange)
            {
                enemy.ChangeState(EnemyStateType.Attack);
            }
            else if (distance > enemy.Config.detectionRange)
            {
                enemy.ChangeState(EnemyStateType.Idle);
            }
        }
        
        public override void FixedUpdate()
        {
            if (enemy.Target == null || rb == null || rb.isKinematic)
                return;
                // Отримуємо напрямок до цілі
                Vector3 direction = (enemy.Target.position - enemy.transform.position).normalized;
                
                // Ігноруємо Y-координату для руху на площині
                direction.y = 0;
                
                // Встановлюємо швидкість через AddForce для більшої стабільності
                Vector3 targetVelocity = new Vector3(direction.x, rb.velocity.y, direction.z) * (enemy.Config.chaseSpeed / 10f);
                Vector3 velocityChange = targetVelocity - rb.velocity;
                velocityChange.y = 0; // Не змінюємо вертикальну швидкість
                
                rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        
        public override void Exit()
        {
            if (rb != null && !rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
            }
        }
    }
    
    // Стан атаки
    public class AttackState : EnemyStateBase
    {
        private float lastAttackTime;
        private Rigidbody rb;
        
        public AttackState(EnemyController enemy) : base(enemy) 
        {
            rb = enemy.GetComponent<Rigidbody>();
        }
        
        public override void Enter()
        {
            // Зупиняємо рух
            if (rb != null && !rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
            }
            
            enemy.SetAnimation("Attack");
            Attack();
        }
        
        public override void Update()
        {
            if (enemy.Target == null)
            {
                enemy.ChangeState(EnemyStateType.Idle);
                return;
            }
            
            // Продовжуємо дивитися на ціль під час атаки
            enemy.LookAtTarget();
            
            float distance = enemy.DistanceToTarget;
            
            // Якщо відстань більша за дистанцію атаки, переходимо до переслідування
            if (distance > enemy.Config.attackRange)
            {
                enemy.ChangeState(EnemyStateType.Chase);
                return;
            }
            
            // Атакуємо з певним кулдауном
            if (Time.time - lastAttackTime >= enemy.Config.attackCooldown)
            {
                Attack();
            }
        }
        
        public override void FixedUpdate()
        {
            // Переконуємося, що ворог залишається на місці під час атаки
            if (rb != null && !rb.isKinematic && rb.velocity.magnitude > 0.1f)
            {
                rb.velocity = Vector3.zero;
            }
        }
        
        private void Attack()
        {
            lastAttackTime = Time.time;
            
            // Start attack animation
            enemy.SetAnimation("Attack");
            
            // Apply damage after delay using UniTask
            ApplyDamageAfterDelay(0.2f).Forget();
        }
        
        private async UniTaskVoid ApplyDamageAfterDelay(float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            
            // Check if target is still valid and within range
            if (enemy.Target != null)
            {
                float currentDistance = enemy.DistanceToTarget;
                
                // Apply damage only if within double the attack range
                if (currentDistance <= enemy.Config.attackRange * 2)
                {
                    var car = enemy.Target.GetComponent<ICarController>();
                    if (car != null)
                    {
                        car.TakeDamage(enemy.Config.attackDamage, enemy.transform.position, enemy.Config.pushForce);
                    }
                }
            }
        }
    }
    
    // Перелік типів станів
    public enum EnemyStateType
    {
        Idle,
        Chase,
        Attack
    }
} 