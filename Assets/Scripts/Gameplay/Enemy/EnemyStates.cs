using System;
using UnityEngine;
using UnityEngine.AI;

namespace TestProject_Factura
{
    // Базовий інтерфейс для станів ворога
    public interface IEnemyState
    {
        void Enter();
        void Update();
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
        private NavMeshAgent navAgent;
        
        public ChaseState(EnemyController enemy) : base(enemy)
        {
            navAgent = enemy.GetComponent<NavMeshAgent>();
        }
        
        public override void Enter()
        {
            enemy.SetAnimation("Run");
            
            if (navAgent != null)
            {
                navAgent.isStopped = false;
                navAgent.speed = enemy.Config.chaseSpeed;
            }
        }
        
        public override void Update()
        {
            if (enemy.Target == null)
            {
                enemy.ChangeState(EnemyStateType.Idle);
                return;
            }
            
            // Оновлюємо цільову позицію для переслідування
            if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.SetDestination(enemy.Target.position);
            }
            
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
        
        public override void Exit()
        {
            if (navAgent != null)
            {
                navAgent.isStopped = true;
            }
        }
    }
    
    // Стан атаки
    public class AttackState : EnemyStateBase
    {
        private float lastAttackTime;
        
        public AttackState(EnemyController enemy) : base(enemy) { }
        
        public override void Enter()
        {
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
        
        private void Attack()
        {
            lastAttackTime = Time.time;
            
            // Наносимо шкоду автомобілю
            var car = enemy.Target.GetComponent<ICarController>();
            if (car != null)
            {
                car.TakeDamage(enemy.Config.attackDamage);
                enemy.SetAnimation("Attack"); // Запускаємо анімацію атаки
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