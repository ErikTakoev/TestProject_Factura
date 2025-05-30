using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace TestProject_Factura
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private ParticleSystem hitEffect;
        
        private float damage;
        private float lifetime = 5f;
        private float lifeTimer;
        private ObjectPool<Bullet> pool;
        
        public void Initialize(Vector3 direction, float speed, float bulletDamage, ObjectPool<Bullet> bulletPool)
        {
            // Зберігаємо посилання на пул
            pool = bulletPool;
            
            // Встановлюємо параметри кулі
            damage = bulletDamage;
            lifeTimer = 0f;
            
            // Скидаємо фізику кулі
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = false;
                rb.AddForce(direction.normalized * speed, ForceMode.VelocityChange);
            }
            
            // Скидаємо трейл, якщо він є
            if (trail != null)
            {
                trail.Clear();
                trail.enabled = true;
            }
            
            // Активуємо об'єкт
            gameObject.SetActive(true);
        }
        
        private void OnEnable()
        {
            lifeTimer = 0f;
        }
        
        private void Update()
        {
            // Відстежуємо час життя кулі
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime)
            {
                ReturnToPool();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Перевіряємо, чи зіткнулися з ворогом
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                // Наносимо шкоду ворогу
                enemy.TakeDamage(damage).Forget();
                
                // Відтворюємо ефект влучання
                PlayHitEffect();
                
                // Повертаємо кулю в пул
                ReturnToPool();
                return;
            }
            
            // Перевіряємо, чи зіткнулися з перешкодою (має тег "Obstacle")
            if (other.CompareTag("Obstacle"))
            {
                // Відтворюємо ефект влучання
                PlayHitEffect();
                
                // Повертаємо кулю в пул
                ReturnToPool();
            }
        }
        
        private void PlayHitEffect()
        {
            if (hitEffect != null)
            {
                // Від'єднуємо ефект від кулі та відтворюємо його
                hitEffect.transform.parent = null;
                hitEffect.Play();
                
                // Знищуємо ефект через деякий час
                Destroy(hitEffect.gameObject, hitEffect.main.duration);
            }
        }
        
        private void ReturnToPool()
        {
            // Вимикаємо трейл, щоб він не відображався при повторному використанні кулі
            if (trail != null)
            {
                trail.enabled = false;
            }
            
            // Зупиняємо рух кулі
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            
            // Повертаємо кулю в пул
            if (pool != null)
            {
                pool.Return(this);
            }
            else
            {
                // Якщо пул не встановлено, просто вимикаємо об'єкт
                gameObject.SetActive(false);
            }
        }
    }
} 